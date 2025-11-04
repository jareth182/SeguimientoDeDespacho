using Microsoft.AspNetCore.Mvc;
using SeguimientoDeDespacho.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization; // <-- 1. AÑADE ESTO

namespace SeguimientoDeDespacho.Controllers
{
    [Authorize] // <-- 2. AÑADE ESTO
    public class HomeController : Controller
    {
        // ... (el resto de tu código de HomeController no cambia)
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Redirigir según el rol del usuario para evitar AccessDenied.
            // - Admin -> PanelLogistico
            // - Cliente -> EstadoEnvio
            // - Otros usuarios autenticados -> Privacy (o cambie a la vista que prefiera)
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "PanelLogistico");
            }

            if (User.IsInRole("Cliente"))
            {
                return RedirectToAction("Index", "EstadoEnvio");
            }

            // Usuario autenticado pero sin rol conocido
            return RedirectToAction("Privacy");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
