using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Models.DTOs;

namespace GestaoPiscina.Server.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey não configurado");
            _issuer = _configuration["Jwt:Issuer"] ?? "GestaoPiscina";
            _audience = _configuration["Jwt:Audience"] ?? "GestaoPiscinaUsers";
        }

        public string GenerateToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IDUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Perfil.Nome),
                new Claim("IDPerfil", usuario.IDPerfil.ToString()),
                new Claim("Login", usuario.Login)
            };

            // Adicionar claims de permissões
            claims.Add(new Claim("PodeGerenciarUsuarios", usuario.Perfil.PodeGerenciarUsuarios.ToString()));
            claims.Add(new Claim("PodeGerenciarClientes", usuario.Perfil.PodeGerenciarClientes.ToString()));
            claims.Add(new Claim("PodeGerenciarPiscinas", usuario.Perfil.PodeGerenciarPiscinas.ToString()));
            claims.Add(new Claim("PodeGerenciarProdutos", usuario.Perfil.PodeGerenciarProdutos.ToString()));
            claims.Add(new Claim("PodeGerenciarEstoque", usuario.Perfil.PodeGerenciarEstoque.ToString()));
            claims.Add(new Claim("PodeGerenciarOrdensServico", usuario.Perfil.PodeGerenciarOrdensServico.ToString()));
            claims.Add(new Claim("PodeGerenciarEquipamentos", usuario.Perfil.PodeGerenciarEquipamentos.ToString()));
            claims.Add(new Claim("PodeVisualizarRelatorios", usuario.Perfil.PodeVisualizarRelatorios.ToString()));
            claims.Add(new Claim("PodeConfigurarSistema", usuario.Perfil.PodeConfigurarSistema.ToString()));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8), // 8 horas
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public UsuarioInfo CreateUsuarioInfo(Usuario usuario)
        {
            return new UsuarioInfo
            {
                IDUsuario = usuario.IDUsuario,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Login = usuario.Login,
                NomePerfil = usuario.Perfil.Nome,
                IDPerfil = usuario.IDPerfil,
                Permissoes = new Dictionary<string, bool>
                {
                    ["PodeGerenciarUsuarios"] = usuario.Perfil.PodeGerenciarUsuarios,
                    ["PodeGerenciarClientes"] = usuario.Perfil.PodeGerenciarClientes,
                    ["PodeGerenciarPiscinas"] = usuario.Perfil.PodeGerenciarPiscinas,
                    ["PodeGerenciarProdutos"] = usuario.Perfil.PodeGerenciarProdutos,
                    ["PodeGerenciarEstoque"] = usuario.Perfil.PodeGerenciarEstoque,
                    ["PodeGerenciarOrdensServico"] = usuario.Perfil.PodeGerenciarOrdensServico,
                    ["PodeGerenciarEquipamentos"] = usuario.Perfil.PodeGerenciarEquipamentos,
                    ["PodeVisualizarRelatorios"] = usuario.Perfil.PodeVisualizarRelatorios,
                    ["PodeConfigurarSistema"] = usuario.Perfil.PodeConfigurarSistema
                }
            };
        }
    }
} 