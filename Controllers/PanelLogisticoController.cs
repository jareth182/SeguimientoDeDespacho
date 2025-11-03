using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- AÑADIR ESTO
using SeguimientoDeDespacho.Data; // <-- AÑADIR ESTO
using SeguimientoDeDespacho.Models; // <-- AÑADIR ESTO

namespace SeguimientoDeDespacho.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PanelLogisticoController : Controller
    {
        // --- INICIO DE MODIFICACIÓN HU03 ---
        private readonly ApplicationDbContext _context;

        public PanelLogisticoController(ApplicationDbContext context) // Inyectar el DbContext
        {
            _context = context;
        }

        // GET: /PanelLogistico/Index
        public async Task<IActionResult> Index() // Convertir a async Task
        {
            // Consultar la base de datos
            var total = await _context.Despachos.CountAsync();
            var enProceso = await _context.Despachos.CountAsync(d => d.Estado == EstadoDespacho.EnProceso);
            var culminados = await _context.Despachos.CountAsync(d => d.Estado == EstadoDespacho.Culminado);
            
            // Calculamos el indicador (Criterio 1 - Tarjeta Verde)
            int rendimiento = (total > 0) ? (int)Math.Round((double)culminados * 100 / total) : 0;

            // Crear el ViewModel
            var viewModel = new PanelLogisticoViewModel
            {
                TotalDespachos = total,
                DespachosEnProceso = enProceso,
                DespachosCulminados = culminados,
                IndicadorRendimiento = rendimiento
            };
            
            // Cumple Criterio 2: Los datos se actualizan al recargar.
            return View(viewModel); // Pasar el ViewModel a la vista
        }
        // --- FIN DE MODIFICACIÓN HU03 ---
    }
}