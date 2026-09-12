using System.Text.Json;
using GestaoPiscina.Server.Data;
using Microsoft.EntityFrameworkCore;
using WebPush;

namespace GestaoPiscina.Server.Services
{
    public interface IPushNotificationService
    {
        Task EnviarParaUsuarioAsync(int idUsuario, string titulo, string corpo, string? url = null);
    }

    // Envia notificações Web Push (VAPID) para todas as inscrições de um usuário. Remove
    // automaticamente inscrições que o navegador já invalidou (404/410 do provedor de push).
    public class PushNotificationService : IPushNotificationService
    {
        private readonly GestaoPiscinaContext _context;
        private readonly WebPushClient _webPushClient;
        private readonly VapidDetails _vapidDetails;
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(GestaoPiscinaContext context, IConfiguration configuration, ILogger<PushNotificationService> logger)
        {
            _context = context;
            _logger = logger;
            _webPushClient = new WebPushClient();

            var publicKey = configuration["WebPush:PublicKey"] ?? throw new InvalidOperationException("Configuração 'WebPush:PublicKey' não encontrada.");
            var privateKey = configuration["WebPush:PrivateKey"] ?? throw new InvalidOperationException("Configuração 'WebPush:PrivateKey' não encontrada.");
            var subject = configuration["WebPush:Subject"] ?? throw new InvalidOperationException("Configuração 'WebPush:Subject' não encontrada.");
            _vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        }

        public async Task EnviarParaUsuarioAsync(int idUsuario, string titulo, string corpo, string? url = null)
        {
            var inscricoes = await _context.PushSubscriptionRegistros
                .Where(p => p.IDUsuario == idUsuario)
                .ToListAsync();

            if (inscricoes.Count == 0)
            {
                return;
            }

            var payload = JsonSerializer.Serialize(new { title = titulo, body = corpo, url });

            foreach (var inscricao in inscricoes)
            {
                var subscription = new PushSubscription(inscricao.Endpoint, inscricao.P256dh, inscricao.Auth);

                try
                {
                    await _webPushClient.SendNotificationAsync(subscription, payload, _vapidDetails);
                }
                catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound || ex.StatusCode == System.Net.HttpStatusCode.Gone)
                {
                    // Inscrição expirada/inválida — remove para não tentar de novo.
                    _context.PushSubscriptionRegistros.Remove(inscricao);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao enviar notificação push para o usuário {IDUsuario}", idUsuario);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
