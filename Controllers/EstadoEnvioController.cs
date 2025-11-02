using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SeguimientoDeDespacho.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class EstadoEnvioController : Controller
    {
        // GET: /EstadoEnvio/Index
        public IActionResult Index()
        {
            // Esta es la vista a la que el Cliente es redirigido
            return View();
        }
    }
}
