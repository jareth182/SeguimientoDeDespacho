using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeguimientoDeDespacho.Data;
using SeguimientoDeDespacho.Models;
using System.Security.Claims;
using System.Text.Json;

namespace SeguimientoDeDespacho.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PanelLogisticoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PanelLogisticoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var total = await _context.Despachos.CountAsync();
            var enProceso = await _context.Despachos.CountAsync(d => d.Estado == EstadoDespacho.EnProceso);
            var culminados = await _context.Despachos.CountAsync(d => d.Estado == EstadoDespacho.Culminado);
            
            int rendimiento = (total > 0) ? (int)Math.Round((double)culminados * 100 / total) : 0;

            var tareasAgendadas = await _context.Tareas
                                    .Where(t => t.Fecha >= DateTime.Today)
                                    .OrderBy(t => t.Fecha)
                                    .Take(5) 
                                    .ToListAsync();
            
            var mensajesDb = await _context.Mensajes
                            .Where(m => m.Leido == false) 
                            .ToListAsync();

            var mensajesNuevos = JsonSerializer.Deserialize<List<Mensaje>>(JsonSerializer.Serialize(mensajesDb)) ?? new List<Mensaje>();

            var viewModel = new PanelLogisticoViewModel
            {
                TotalDespachos = total,
                DespachosEnProceso = enProceso,
                DespachosCulminados = culminados,
                IndicadorRendimiento = rendimiento,
                TareasAgendadas = tareasAgendadas,
                MensajesNuevos = mensajesNuevos 
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken] // Seguridad
        public async Task<IActionResult> GuardarEvento(string titulo, DateTime fecha)
        {
            if (string.IsNullOrEmpty(titulo) || fecha == default)
            {
                return Json(new { success = false, message = "Los datos del evento están incompletos." });
            }

            var nuevaTarea = new Tarea
            {
                Titulo = titulo,
                Fecha = fecha
            };

            _context.Tareas.Add(nuevaTarea);
            await _context.SaveChangesAsync();

            return Json(new { success = true, tarea = nuevaTarea });
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerEventos()
        {
            var tareas = await _context.Tareas.ToListAsync();

            var eventos = tareas.Select(t => new 
            {
                id = t.Id,
                title = t.Titulo,
                start = t.Fecha.ToString("yyyy-MM-dd")
            });

            return Json(eventos);
        }

    }
}