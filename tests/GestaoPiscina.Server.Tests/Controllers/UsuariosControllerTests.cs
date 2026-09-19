using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using GestaoPiscina.Server.Controllers;
using GestaoPiscina.Server.Services;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Controllers
{
    // Endereço obrigatório por perfil (Perfil.ExigeEndereco) no cadastro de usuário.
    public class UsuariosControllerTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();
        private readonly JwtService _jwtService;

        public UsuariosControllerTests()
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

        private UsuariosController CriarController() => new(_db.Context, _jwtService);

        [Fact]
        public async Task PostUsuario_PerfilExigeEnderecoSemEndereco_RetornaBadRequest()
        {
            var perfil = Fabrica.Perfil("Técnico", exigeEndereco: true);
            _db.Context.Perfis.Add(perfil);
            await _db.Context.SaveChangesAsync();

            var dto = new CriarUsuarioDTO
            {
                Nome = "Novo Técnico",
                Email = "novo.tecnico@teste.com",
                Login = "novo.tecnico",
                SenhaInicial = "Senha@123",
                IDPerfil = perfil.IDPerfil
            };

            var resultado = await CriarController().PostUsuario(dto);

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostUsuario_PerfilExigeEnderecoComEndereco_CriaComSucesso()
        {
            var perfil = Fabrica.Perfil("Técnico", exigeEndereco: true);
            _db.Context.Perfis.Add(perfil);
            await _db.Context.SaveChangesAsync();

            var dto = new CriarUsuarioDTO
            {
                Nome = "Novo Técnico",
                Email = "novo.tecnico@teste.com",
                Login = "novo.tecnico",
                SenhaInicial = "Senha@123",
                IDPerfil = perfil.IDPerfil,
                Endereco = "Rua Teste, 123"
            };

            var resultado = await CriarController().PostUsuario(dto);

            var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.IsType<UsuarioAdminDTO>(created.Value);
        }

        [Fact]
        public async Task PostUsuario_PerfilNaoExigeEnderecoSemEndereco_CriaComSucesso()
        {
            var perfil = Fabrica.Perfil("Cliente", exigeEndereco: false);
            _db.Context.Perfis.Add(perfil);
            await _db.Context.SaveChangesAsync();

            var dto = new CriarUsuarioDTO
            {
                Nome = "Novo Cliente",
                Email = "novo.cliente@teste.com",
                Login = "novo.cliente",
                SenhaInicial = "Senha@123",
                IDPerfil = perfil.IDPerfil
            };

            var resultado = await CriarController().PostUsuario(dto);

            Assert.IsType<CreatedAtActionResult>(resultado.Result);
        }

        [Fact]
        public async Task PutUsuario_TrocaParaPerfilQueExigeEnderecoSemEndereco_RetornaBadRequest()
        {
            var perfilSemEndereco = Fabrica.Perfil("Cliente", exigeEndereco: false);
            var perfilComEndereco = Fabrica.Perfil("Supervisor", exigeEndereco: true);
            var usuario = Fabrica.Usuario(perfilSemEndereco, login: "usuario1");
            _db.Context.AddRange(perfilSemEndereco, perfilComEndereco, usuario);
            await _db.Context.SaveChangesAsync();

            var dto = new AtualizarUsuarioDTO
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                Login = usuario.Login,
                IDPerfil = perfilComEndereco.IDPerfil,
                Ativo = true
            };

            var resultado = await CriarController().PutUsuario(usuario.IDUsuario, dto);

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        public void Dispose() => _db.Dispose();
    }
}
