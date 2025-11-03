using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SeguimientoDeDespacho.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ListadoDespachosController : Controller
    {
        // GET: /ListadoDespachos/Index
        // Esta acción recibe el filtro del Criterio 3
        public IActionResult Index(string filtroEstado)
        {
            ViewData["FiltroAplicado"] = filtroEstado;
            return View();
        }
    }
}