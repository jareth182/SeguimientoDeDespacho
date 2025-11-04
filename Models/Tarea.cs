using System.ComponentModel.DataAnnotations;

namespace SeguimientoDeDespacho.Models
{
    public class Tarea
    {
        [Key]
        public int Id { get; set; }
        public string Titulo { get; set; }
        public DateTime Fecha { get; set; }
        // public string UsuarioId { get; set; } // Opcional: para saber quién la creó
    }
}