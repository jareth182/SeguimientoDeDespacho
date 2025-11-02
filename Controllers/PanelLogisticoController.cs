using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SeguimientoDeDespacho.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PanelLogisticoController : Controller
    {
        // GET: /PanelLogistico/Index
        public IActionResult Index()
        {
            // Esta es la vista a la que el Admin es redirigido
            return View();
        }
    }
}
