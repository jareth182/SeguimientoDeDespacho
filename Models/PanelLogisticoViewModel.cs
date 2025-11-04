using System;
using System.Collections.Generic;

namespace SeguimientoDeDespacho.Models
{
    public class PanelLogisticoViewModel
    {
        public int TotalDespachos { get; set; }
        public int DespachosEnProceso { get; set; }
        public int DespachosCulminados { get; set; }
        public int IndicadorRendimiento { get; set; }
        public List<Tarea> TareasAgendadas { get; set; } = new List<Tarea>();
        public List<Mensaje> MensajesNuevos { get; set; } = new List<Mensaje>();
    }

    // Minimal Mensaje class so the ViewModel compiles.
    public class Mensaje
    {
        public int Id { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}