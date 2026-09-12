using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using GestaoPiscina.Server.Controllers;
using GestaoPiscina.Server.Models.DTOs;
using GestaoPiscina.Server.Services;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Controllers
{
    // RNF04 – Segurança e Autenticação.
    public class AuthControllerTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();
        private readonly JwtService _jwtService;

        public AuthControllerTests()
        {
            var configuracao = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "chave-secreta-de-teste-com-32-caracteres-ou-mais",
                    ["Jwt:Issuer"] = "GestaoPiscinaTeste",
                    ["Jwt:Audience"] = "GestaoPiscinaTesteUsers"
                })
                .Build();
            _jwtService = new JwtService(configuracao);
        }

        private AuthController CriarController() => new(_db.Context, _jwtService);

        [Fact]
        public async Task Login_ComCredenciaisValidas_RetornaTokenEAtualizaUltimoAcesso()
        {
            var perfil = Fabrica.Perfil("Técnico");
            var usuario = Fabrica.Usuario(perfil, login: "joao", senha: "Senha@123");
            _db.Context.AddRange(perfil, usuario);
            await _db.Context.SaveChangesAsync();
            Assert.Null(usuario.UltimoAcesso);

            var resultado = await CriarController().Login(new LoginRequest { Login = "joao", Senha = "Senha@123" });

            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var resposta = Assert.IsType<LoginResponse>(ok.Value);
            Assert.True(resposta.Sucesso);
            Assert.False(string.IsNullOrEmpty(resposta.Token));
            Assert.Equal("joao", resposta.Usuario!.Login);
            Assert.NotNull(usuario.UltimoAcesso);
        }

        [Fact]
        public async Task Login_ComSenhaIncorreta_RetornaUnauthorized()
        {
            var perfil = Fabrica.Perfil();
            var usuario = Fabrica.Usuario(perfil, login: "joao", senha: "Senha@123");
            _db.Context.AddRange(perfil, usuario);
            await _db.Context.SaveChangesAsync();

            var resultado = await CriarController().Login(new LoginRequest { Login = "joao", Senha = "SenhaErrada" });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(resultado.Result);
            var resposta = Assert.IsType<LoginResponse>(unauthorized.Value);
            Assert.False(resposta.Sucesso);
        }

        [Fact]
        public async Task Login_ComLoginInexistente_RetornaUnauthorized()
        {
            var resultado = await CriarController().Login(new LoginRequest { Login = "naoexiste", Senha = "qualquer" });

            Assert.IsType<UnauthorizedObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task Login_ComUsuarioInativo_RetornaUnauthorized()
        {
            var perfil = Fabrica.Perfil();
            var usuario = Fabrica.Usuario(perfil, login: "joao", senha: "Senha@123", ativo: false);
            _db.Context.AddRange(perfil, usuario);
            await _db.Context.SaveChangesAsync();

            var resultado = await CriarController().Login(new LoginRequest { Login = "joao", Senha = "Senha@123" });

            Assert.IsType<UnauthorizedObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task AlterarSenha_ComSenhaAtualCorreta_AlteraEPermiteLoginComANova()
        {
            var perfil = Fabrica.Perfil();
            var usuario = Fabrica.Usuario(perfil, login: "joao", senha: "SenhaAntiga1");
            _db.Context.AddRange(perfil, usuario);
            await _db.Context.SaveChangesAsync();
            var controller = CriarControllerAutenticadoComo(usuario.IDUsuario);

            var resultado = await controller.AlterarSenha(new AlterarSenhaRequest
            {
                SenhaAtual = "SenhaAntiga1",
                NovaSenha = "SenhaNova2",
                ConfirmarSenha = "SenhaNova2"
            });

            Assert.IsType<OkObjectResult>(resultado);
            var loginComNova = await CriarController().Login(new LoginRequest { Login = "joao", Senha = "SenhaNova2" });
            Assert.IsType<OkObjectResult>(loginComNova.Result);
        }

        [Fact]
        public async Task AlterarSenha_ComSenhaAtualIncorreta_RetornaBadRequestSemAlterarNada()
        {
            var perfil = Fabrica.Perfil();
            var usuario = Fabrica.Usuario(perfil, login: "joao", senha: "SenhaAntiga1");
            _db.Context.AddRange(perfil, usuario);
            await _db.Context.SaveChangesAsync();
            var controller = CriarControllerAutenticadoComo(usuario.IDUsuario);

            var resultado = await controller.AlterarSenha(new AlterarSenhaRequest
            {
                SenhaAtual = "SenhaErrada",
                NovaSenha = "SenhaNova2",
                ConfirmarSenha = "SenhaNova2"
            });

            Assert.IsType<BadRequestObjectResult>(resultado);
            var loginComAntiga = await CriarController().Login(new LoginRequest { Login = "joao", Senha = "SenhaAntiga1" });
            Assert.IsType<OkObjectResult>(loginComAntiga.Result);
        }

        private AuthController CriarControllerAutenticadoComo(int idUsuario)
        {
            var controller = CriarController();
            var claims = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()) });
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(claims) }
            };
            return controller;
        }

        public void Dispose() => _db.Dispose();
    }
}
