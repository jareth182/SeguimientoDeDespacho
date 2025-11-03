// This is a scaffolded file. We have modified it to meet HU01 requirements.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SeguimientoDeDespacho.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<IdentityUser> _userManager; 

        public LoginModel(SignInManager<IdentityUser> signInManager,
                          ILogger<LoginModel> logger,
                          UserManager<IdentityUser> userManager) 
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager; 
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Este campo es obligatorio")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Este campo es obligatorio")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Recordarme")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            ReturnUrl = returnUrl ?? Url.Content("~/");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "El correo no está registrado en el sistema.");
                    
                    // 1. Borramos el email del modelo
                    Input.Email = string.Empty; 
                    
                    // 2. Borramos el email del "estado del formulario" (¡ESTA ES LA LÍNEA NUEVA!)
                    ModelState.Remove("Input.Email");
                    
                    return Page();
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: Input.RememberMe);
                    _logger.LogInformation("Usuario conectado.");
                    
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "PanelLogistico");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Cliente"))
                    {
                        return RedirectToAction("Index", "EstadoEnvio");
                    }
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    // Contraseña incorrecta, el email se queda (no borramos el ModelState)
                    ModelState.AddModelError(string.Empty, "Contraseña incorrecta.");
                    return Page();
                }
            }
            return Page();
        }
    }
}

