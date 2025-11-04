// Models/Mensaje.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace SeguimientoDeDespacho.Models
{
    public class MensajeModel
    {
        [Key]
        public int Id { get; set; }
        public string Contenido { get; set; }
        public DateTime FechaEnvio { get; set; }
        public bool Leido { get; set; } = false;
        
        // Puedes añadir de quién es y para quién es
        // public string RemitenteId { get; set; }
        // public string DestinatarioId { get; set; }
    }
}