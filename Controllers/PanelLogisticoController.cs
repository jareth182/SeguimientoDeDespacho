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
        
        // --- CAMBIO: Lógica para aceptar 'hora' ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarEvento(string titulo, string fecha, string hora) // <-- Argumentos cambiados
        {
            // Validar la entrada
            if (string.IsNullOrEmpty(titulo) || string.IsNullOrEmpty(fecha))
            {
                return Json(new { success = false, message = "Los datos del evento están incompletos." });
            }

            // Pone medianoche si el campo 'hora' está vacío
            if (string.IsNullOrEmpty(hora))
            {
                hora = "00:00"; 
            }

            // Intentamos combinar la fecha (ej. "2025-11-04") y la hora (ej. "14:30")
            if (!DateTime.TryParse($"{fecha} {hora}", out DateTime fechaCompleta))
            {
                 return Json(new { success = false, message = "La fecha o la hora tienen un formato incorrecto." });
            }
            // --- FIN DEL CAMBIO ---

            var nuevaTarea = new Tarea
            {
                Titulo = titulo,
                Fecha = fechaCompleta // <-- Guardamos el DateTime combinado
            };

            _context.Tareas.Add(nuevaTarea);
            await _context.SaveChangesAsync();

            return Json(new { success = true, tarea = nuevaTarea });
        }

        // --- CAMBIO: Lógica para enviar la 'hora' al calendario ---
        [HttpGet]
        public async Task<IActionResult> ObtenerEventos()
        {
            var tareas = await _context.Tareas.ToListAsync();

            var eventos = tareas.Select(t => new
            {
                id = t.Id,
                title = t.Titulo,
                // "o" es el formato ISO 8601 que incluye la hora (ej: "2025-11-04T14:30:00")
                start = t.Fecha.ToString("o") // <-- Línea modificada
            });

            return Json(eventos);
        }
        // --- FIN DEL CAMBIO ---
        
        public async Task<IActionResult> EventoDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea == null)
            {
                return NotFound();
            }
            
            return View(tarea); 
        }

        [HttpPost, ActionName("EliminarEvento")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEventoConfirmado(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea != null)
            {
                _context.Tareas.Remove(tarea);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index)); 
        }

    }
}