using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Services
{
    // RNF04 – Segurança e Autenticação: controle de sessão via token assinado.
    public class JwtServiceTests
    {
        private static JwtService CriarServico(string chaveSecreta = "chave-secreta-de-teste-com-32-caracteres-ou-mais")
        {
            var configuracao = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = chaveSecreta,
                    ["Jwt:Issuer"] = "GestaoPiscinaTeste",
                    ["Jwt:Audience"] = "GestaoPiscinaTesteUsers"
                })
                .Build();

            return new JwtService(configuracao);
        }

        [Fact]
        public void Constructor_SemSecretKeyConfigurada_LancaExcecao()
        {
            var configuracao = new ConfigurationBuilder().Build();

            Assert.Throws<InvalidOperationException>(() => new JwtService(configuracao));
        }

        [Fact]
        public void GenerateToken_IncluiClaimsDePerfilEPermissoes()
        {
            var servico = CriarServico();
            var perfil = Fabrica.Perfil("Técnico");
            perfil.IDPerfil = 1;
            var usuario = Fabrica.Usuario(perfil, login: "joao");
            usuario.IDUsuario = 42;

            var token = servico.GenerateToken(usuario);

            var lido = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Equal("42", lido.Claims.First(c => c.Type == "nameid").Value);
            Assert.Equal("joao", lido.Claims.First(c => c.Type == "Login").Value);
            Assert.Equal("Técnico", lido.Claims.First(c => c.Type == "role").Value);
            Assert.Equal("True", lido.Claims.First(c => c.Type == "PodeGerenciarOrdensServico").Value);
            Assert.Equal("False", lido.Claims.First(c => c.Type == "PodeGerenciarUsuarios").Value);
        }

        [Fact]
        public void ValidateToken_ComTokenGeradoPeloMesmoServico_RetornaPrincipalValido()
        {
            var servico = CriarServico();
            var perfil = Fabrica.Perfil("Gestor");
            var usuario = Fabrica.Usuario(perfil, login: "gestor1");
            usuario.IDUsuario = 7;

            var token = servico.GenerateToken(usuario);
            var principal = servico.ValidateToken(token);

            Assert.NotNull(principal);
            Assert.Equal("7", principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        }

        [Fact]
        public void ValidateToken_AssinadoComChaveDiferente_RetornaNull()
        {
            var servicoEmissor = CriarServico(chaveSecreta: "primeira-chave-secreta-com-32-caracteres!");
            var servicoValidador = CriarServico(chaveSecreta: "segunda-chave-secreta-completamente-diferente!");
            var perfil = Fabrica.Perfil();
            var usuario = Fabrica.Usuario(perfil);

            var token = servicoEmissor.GenerateToken(usuario);
            var principal = servicoValidador.ValidateToken(token);

            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_TokenInvalido_RetornaNull()
        {
            var servico = CriarServico();

            var principal = servico.ValidateToken("token-completamente-invalido");

            Assert.Null(principal);
        }

        [Fact]
        public void CreateUsuarioInfo_ExpoeNomeDoPerfilEPermissoes()
        {
            var servico = CriarServico();
            var perfil = Fabrica.Perfil("Supervisor");
            var usuario = Fabrica.Usuario(perfil, login: "supervisor1");

            var info = servico.CreateUsuarioInfo(usuario);

            Assert.Equal("Supervisor", info.NomePerfil);
            Assert.True(info.Permissoes["PodeGerenciarOrdensServico"]);
        }
    }
}
