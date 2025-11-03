using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace SeguimientoDeDespacho.Services
{
    // Esto simula el envío de un correo para desarrollo.
    // Imprime el enlace de reseteo en la consola.
    public class DummyEmailSender : IEmailSender
    {
        private readonly ILogger<DummyEmailSender> _logger;

        public DummyEmailSender(ILogger<DummyEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogWarning($"--- SIMULACIÓN DE ENVÍO DE CORREO ---");
            _logger.LogInformation($"Para: {email}");
            _logger.LogInformation($"Asunto: {subject}");
            _logger.LogInformation($"Mensaje (contiene el enlace): {htmlMessage}");
            _logger.LogWarning($"--- FIN DE SIMULACIÓN ---");

            return Task.CompletedTask;
        }
    }
}
