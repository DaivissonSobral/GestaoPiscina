using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Models.DTOs;
using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;
        private readonly JwtService _jwtService;

        public AuthController(GestaoPiscinaContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validar dados de entrada
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponse
                    {
                        Sucesso = false,
                        Mensagem = "Dados de entrada inválidos"
                    });
                }

                // Buscar usuário com perfil
                var usuario = await _context.Usuarios
                    .Include(u => u.Perfil)
                    .FirstOrDefaultAsync(u => u.Login == request.Login && u.Ativo);

                if (usuario == null)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Sucesso = false,
                        Mensagem = "Usuário não encontrado ou inativo"
                    });
                }

                // Verificar senha
                if (!VerifyPassword(request.Senha, usuario.SenhaHash))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Sucesso = false,
                        Mensagem = "Senha incorreta"
                    });
                }

                // Atualizar último acesso
                usuario.UltimoAcesso = DateTime.Now;
                await _context.SaveChangesAsync();

                // Gerar token
                var token = _jwtService.GenerateToken(usuario);
                var refreshToken = _jwtService.GenerateRefreshToken();

                return Ok(new LoginResponse
                {
                    Sucesso = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiraEm = DateTime.UtcNow.AddHours(8),
                    Usuario = _jwtService.CreateUsuarioInfo(usuario)
                });
            }
            catch
            {
                return StatusCode(500, new LoginResponse
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor"
                });
            }
        }

        [HttpPost("refresh")]
        public Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                // Aqui você implementaria a lógica para validar o refresh token
                // Por simplicidade, vamos apenas retornar um erro
                return Task.FromResult<ActionResult<LoginResponse>>(Unauthorized(new LoginResponse
                {
                    Sucesso = false,
                    Mensagem = "Refresh token não implementado"
                }));
            }
            catch
            {
                return Task.FromResult<ActionResult<LoginResponse>>(StatusCode(500, new LoginResponse
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor"
                }));
            }
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            // Em uma implementação real, você invalidaria o token
            return Ok(new { message = "Logout realizado com sucesso" });
        }

        [HttpGet("me")]
        public async Task<ActionResult<UsuarioInfo>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var usuario = await _context.Usuarios
                    .Include(u => u.Perfil)
                    .FirstOrDefaultAsync(u => u.IDUsuario == userId && u.Ativo);

                if (usuario == null)
                {
                    return Unauthorized();
                }

                return Ok(_jwtService.CreateUsuarioInfo(usuario));
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("alterar-senha")]
        public async Task<ActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario == null)
                {
                    return Unauthorized();
                }

                // Verificar senha atual
                if (!VerifyPassword(request.SenhaAtual, usuario.SenhaHash))
                {
                    return BadRequest(new { message = "Senha atual incorreta" });
                }

                // Hash da nova senha
                usuario.SenhaHash = HashPassword(request.NovaSenha);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Senha alterada com sucesso" });
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
} 