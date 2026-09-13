using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GestaoPiscina.Server.Controllers;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Controllers
{
    // Inscrição de navegadores/dispositivos para notificação Web Push (VAPID) — ver
    // PushSubscriptionsController.
    public class PushSubscriptionsControllerTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();

        private static IConfiguration ConfiguracaoComChaves(string? publicKey = "chave-publica-teste") =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["WebPush:PublicKey"] = publicKey,
                    ["WebPush:PrivateKey"] = "chave-privada-teste",
                    ["WebPush:Subject"] = "mailto:teste@teste.com"
                })
                .Build();

        private PushSubscriptionsController Controller(IConfiguration? configuracao = null) =>
            new(_db.Context, configuracao ?? ConfiguracaoComChaves());

        private PushSubscriptionsController ControllerAutenticadoComo(int idUsuario)
        {
            var controller = Controller();
            var claims = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()) });
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(claims) }
            };
            return controller;
        }

        [Fact]
        public void GetVapidPublicKey_ComChaveConfigurada_RetornaAChave()
        {
            var resultado = Controller().GetVapidPublicKey();

            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.Equal("chave-publica-teste", ok.Value);
        }

        [Fact]
        public void GetVapidPublicKey_SemChaveConfigurada_RetornaNotFound()
        {
            var resultado = Controller(new ConfigurationBuilder().Build()).GetVapidPublicKey();

            Assert.IsType<NotFoundResult>(resultado.Result);
        }

        [Fact]
        public async Task PostSubscription_ComUsuarioAutenticado_CriaInscricao()
        {
            var perfil = Fabrica.Perfil("Química");
            var usuario = Fabrica.Usuario(perfil, login: "quimica1");
            _db.Context.AddRange(perfil, usuario);
            await _db.Context.SaveChangesAsync();

            var resultado = await ControllerAutenticadoComo(usuario.IDUsuario).PostSubscription(new PushSubscriptionRequest
            {
                Endpoint = "https://push.exemplo.com/abc",
                Keys = new PushSubscriptionRequest.PushSubscriptionKeys { P256dh = "chave-p256dh", Auth = "chave-auth" }
            });

            Assert.IsType<NoContentResult>(resultado);
            var inscricao = await _db.Context.PushSubscriptionRegistros.AsNoTracking().SingleAsync();
            Assert.Equal(usuario.IDUsuario, inscricao.IDUsuario);
            Assert.Equal("https://push.exemplo.com/abc", inscricao.Endpoint);
        }

        [Fact]
        public async Task PostSubscription_SemClaimDeUsuario_RetornaUnauthorized()
        {
            var controller = Controller();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            var resultado = await controller.PostSubscription(new PushSubscriptionRequest
            {
                Endpoint = "https://push.exemplo.com/abc",
                Keys = new PushSubscriptionRequest.PushSubscriptionKeys { P256dh = "x", Auth = "y" }
            });

            Assert.IsType<UnauthorizedResult>(resultado);
        }

        [Fact]
        public async Task PostSubscription_ComEndpointJaExistente_AtualizaEmVezDeDuplicar()
        {
            // Mesmo dispositivo (endpoint do PushManager), outro usuário loga depois — ex.:
            // troca de turno no mesmo tablet compartilhado. A inscrição deve ser reatribuída,
            // não duplicada.
            var perfil = Fabrica.Perfil("Química");
            var usuarioAntigo = Fabrica.Usuario(perfil, login: "quimica1");
            var usuarioNovo = Fabrica.Usuario(perfil, login: "quimica2", nome: "Química Dois");
            _db.Context.AddRange(perfil, usuarioAntigo, usuarioNovo);
            await _db.Context.SaveChangesAsync();
            const string endpoint = "https://push.exemplo.com/mesmo-dispositivo";

            await ControllerAutenticadoComo(usuarioAntigo.IDUsuario).PostSubscription(new PushSubscriptionRequest
            {
                Endpoint = endpoint,
                Keys = new PushSubscriptionRequest.PushSubscriptionKeys { P256dh = "antiga", Auth = "antiga" }
            });
            await ControllerAutenticadoComo(usuarioNovo.IDUsuario).PostSubscription(new PushSubscriptionRequest
            {
                Endpoint = endpoint,
                Keys = new PushSubscriptionRequest.PushSubscriptionKeys { P256dh = "nova", Auth = "nova" }
            });

            var inscricao = await _db.Context.PushSubscriptionRegistros.AsNoTracking().SingleAsync();
            Assert.Equal(usuarioNovo.IDUsuario, inscricao.IDUsuario);
            Assert.Equal("nova", inscricao.P256dh);
        }

        [Fact]
        public async Task PostSubscription_ComChavesFaltando_RetornaBadRequest()
        {
            var perfil = Fabrica.Perfil("Química");
            var usuario = Fabrica.Usuario(perfil, login: "quimica1");
            _db.Context.AddRange(perfil, usuario);
            await _db.Context.SaveChangesAsync();

            var resultado = await ControllerAutenticadoComo(usuario.IDUsuario).PostSubscription(new PushSubscriptionRequest
            {
                Endpoint = "https://push.exemplo.com/abc",
                Keys = null
            });

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        public void Dispose() => _db.Dispose();
    }
}
