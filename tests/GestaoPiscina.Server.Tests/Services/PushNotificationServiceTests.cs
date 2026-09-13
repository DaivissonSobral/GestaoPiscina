using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using GestaoPiscina.Server.Services;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Services
{
    // PushNotificationService envia notificações Web Push (VAPID) reais via WebPushClient —
    // sem infraestrutura de mock de HTTP no projeto, os testes aqui cobrem só a parte
    // determinística e sem rede: validação das chaves VAPID na construção e o caminho de
    // saída antecipada quando o usuário não tem nenhuma inscrição.
    public class PushNotificationServiceTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();

        private static IConfiguration ConfiguracaoComChaves() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["WebPush:PublicKey"] = "chave-publica-teste",
                    ["WebPush:PrivateKey"] = "chave-privada-teste",
                    ["WebPush:Subject"] = "mailto:teste@teste.com"
                })
                .Build();

        private PushNotificationService CriarServico(IConfiguration? configuracao = null) => new(
            _db.Context,
            configuracao ?? ConfiguracaoComChaves(),
            NullLogger<PushNotificationService>.Instance);

        [Fact]
        public async Task EnviarParaUsuarioAsync_SemInscricoesParaOUsuario_NaoLancaExcecaoNemContactaOProvedor()
        {
            var servico = CriarServico();

            var excecao = await Record.ExceptionAsync(() => servico.EnviarParaUsuarioAsync(999, "Título", "Corpo"));

            Assert.Null(excecao);
        }

        [Theory]
        [InlineData("WebPush:PublicKey")]
        [InlineData("WebPush:PrivateKey")]
        [InlineData("WebPush:Subject")]
        public void Construtor_SemUmaDasChavesVapidConfiguradas_LancaInvalidOperationException(string chaveFaltando)
        {
            var valores = new Dictionary<string, string?>
            {
                ["WebPush:PublicKey"] = "chave-publica-teste",
                ["WebPush:PrivateKey"] = "chave-privada-teste",
                ["WebPush:Subject"] = "mailto:teste@teste.com"
            };
            valores.Remove(chaveFaltando);
            var configuracaoIncompleta = new ConfigurationBuilder().AddInMemoryCollection(valores).Build();

            Assert.Throws<InvalidOperationException>(() => CriarServico(configuracaoIncompleta));
        }

        public void Dispose() => _db.Dispose();
    }
}
