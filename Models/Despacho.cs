using System.ComponentModel.DataAnnotations;

namespace SeguimientoDeDespacho.Models
{
    public class Despacho
    {
        public int Id { get; set; }

        [Required]
        public string NumeroGuia { get; set; }

        [Required]
        public string ClienteNombre { get; set; }

        [Required]
        public EstadoDespacho Estado { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}