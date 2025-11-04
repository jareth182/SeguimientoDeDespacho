using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SeguimientoDeDespacho.Models;

namespace SeguimientoDeDespacho.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Despacho> Despachos { get; set; }
        
        // --- AÑADIR ESTA LÍNEA PARA HU05 ---
        public DbSet<Tarea> Tareas { get; set; } 
        public DbSet<MensajeModel> Mensajes { get; set; }
    }
}