using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Tests.TestSupport
{
    // Dublê sem operação — grava as chamadas recebidas (em vez de realmente enviar nada)
    // para os testes que precisam verificar quem foi notificado, e serve como no-op para os
    // que só precisam que o controller não quebre ao chamá-lo.
    public class FakePushNotificationService : IPushNotificationService
    {
        public List<(int IdUsuario, string Titulo, string Corpo, string? Url)> Chamadas { get; } = new();

        public Task EnviarParaUsuarioAsync(int idUsuario, string titulo, string corpo, string? url = null)
        {
            Chamadas.Add((idUsuario, titulo, corpo, url));
            return Task.CompletedTask;
        }
    }
}
