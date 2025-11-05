using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeguimientoDeDespacho.Data;
using SeguimientoDeDespacho.Models;
using System.Linq; 
using System.Threading.Tasks;

public class ListadoDespachosController : Controller
{
    private readonly ApplicationDbContext _context;

    public ListadoDespachosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Esta es tu acción existente (sin cambios)
    public async Task<IActionResult> Index(string filtroEstado)
    {
        IQueryable<Despacho> query = _context.Despachos.AsQueryable();

        if (!string.IsNullOrEmpty(filtroEstado))
        {
            if (Enum.TryParse<EstadoDespacho>(filtroEstado, out var estadoFiltrado))
            {
                query = query.Where(d => d.Estado == estadoFiltrado);
                ViewData["FiltroAplicado"] = filtroEstado;
            }
        }

        var listadoDeDespachos = await query
                                    .OrderByDescending(d => d.FechaCreacion)
                                    .ToListAsync();

        return View(listadoDeDespachos);
    }

    // --- AÑADIR ESTA NUEVA ACCIÓN ---
    // GET: ListadoDespachos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound(); // Si no nos pasan un ID
        }

        // Busca el despacho en la BD usando el ID
        var despacho = await _context.Despachos
            .FirstOrDefaultAsync(m => m.Id == id);

        if (despacho == null)
        {
            return NotFound(); // Si no se encontró un despacho con ese ID
        }

        // Envía el despacho encontrado a la nueva vista "Details.cshtml"
        return View(despacho);
    }
    // --- FIN DE LA NUEVA ACCIÓN ---
}