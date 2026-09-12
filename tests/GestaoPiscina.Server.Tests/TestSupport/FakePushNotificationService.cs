using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Tests.TestSupport
{
    // Dublê sem operação — os testes de OrdensDeServicoController não precisam verificar
    // o envio de push, só que o controller não quebra ao chamá-lo.
    public class FakePushNotificationService : IPushNotificationService
    {
        public Task EnviarParaUsuarioAsync(int idUsuario, string titulo, string corpo, string? url = null)
            => Task.CompletedTask;
    }
}
