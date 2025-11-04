using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeguimientoDeDespacho.Data;
using SeguimientoDeDespacho.Models;
using System.Security.Claims; // <-- AÑADIR ESTO (Opcional, para ID de usuario)
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

        // GET: /PanelLogistico/Index
        public async Task<IActionResult> Index()
        {
            // --- INICIO LÓGICA HU03 (Tu código existente) ---
            var total = await _context.Despachos.CountAsync();
            var enProceso = await _context.Despachos.CountAsync(d => d.Estado == EstadoDespacho.EnProceso);
            var culminados = await _context.Despachos.CountAsync(d => d.Estado == EstadoDespacho.Culminado);
            
            int rendimiento = (total > 0) ? (int)Math.Round((double)culminados * 100 / total) : 0;
            // --- FIN LÓGICA HU03 ---

            
            // --- INICIO LÓGICA HU05 (CRITERIO 1: Cargar listas) ---
            // Carga las 5 tareas más próximas (a partir de hoy)
            var tareasAgendadas = await _context.Tareas
                                    .Where(t => t.Fecha >= DateTime.Today)
                                    .OrderBy(t => t.Fecha)
                                    .Take(5) 
                                    .ToListAsync();
            // Aquí iría la lógica para cargar mensajes nuevos
            var mensajesDb = await _context.Mensajes
                            .Where(m => m.Leido == false) // Filtra solo los no leídos
                            .ToListAsync();

            // Convertir lista de MensajeModel a List<Mensaje> para que coincida con el ViewModel.
            // Usamos serialización para mapear propiedades con nombres coincidentes.
            var mensajesNuevos = JsonSerializer.Deserialize<List<Mensaje>>(JsonSerializer.Serialize(mensajesDb)) ?? new List<Mensaje>();
            // --- FIN LÓGICA HU05 ---


            // Crear el ViewModel con TODOS los datos
            var viewModel = new PanelLogisticoViewModel
            {
                // Datos de HU03
                TotalDespachos = total,
                DespachosEnProceso = enProceso,
                DespachosCulminados = culminados,
                IndicadorRendimiento = rendimiento,

                // Datos de HU05
                TareasAgendadas = tareasAgendadas,
                MensajesNuevos = mensajesNuevos 
            };
            
            return View(viewModel); // Pasar el ViewModel completo a la vista
        }
        
        // --- INICIO LÓGICA HU05 (CRITERIO 2: Guardar Tarea) ---
        // POST: /PanelLogistico/GuardarEvento
        [HttpPost]
        [ValidateAntiForgeryToken] // Seguridad
        public async Task<IActionResult> GuardarEvento(string titulo, DateTime fecha)
        {
            // Validar la entrada
            if (string.IsNullOrEmpty(titulo) || fecha == default)
            {
                return Json(new { success = false, message = "Los datos del evento están incompletos." });
            }

            // Opcional: Obtener el ID del usuario que crea la tarea
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var nuevaTarea = new Tarea
            {
                Titulo = titulo,
                Fecha = fecha
                // UsuarioId = userId 
            };

            // Guardar en la Base de Datos
            _context.Tareas.Add(nuevaTarea);
            await _context.SaveChangesAsync();

            // Devolver la tarea creada (con su nuevo Id) al script de AJAX
            return Json(new { success = true, tarea = nuevaTarea });
        }
        // --- FIN LÓGICA HU05 ---
    }
}