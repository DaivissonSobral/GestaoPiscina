using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Models.DTOs;
using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;
        private readonly JwtService _jwtService;

        public UsuariosController(GestaoPiscinaContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // Somente leitura, usado para popular seletores de usuário (ex: técnico/aprovador
        // na OS). Só usuários ativos. Nunca retorna SenhaHash.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResumoDTO>>> GetUsuarios([FromQuery] string? perfil)
        {
            var query = _context.Usuarios.Include(u => u.Perfil).Where(u => u.Ativo);

            if (!string.IsNullOrWhiteSpace(perfil))
            {
                var perfis = perfil.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                query = query.Where(u => perfis.Contains(u.Perfil.Nome));
            }

            var usuarios = await query
                .OrderBy(u => u.Nome)
                .Select(u => new UsuarioResumoDTO
                {
                    IDUsuario = u.IDUsuario,
                    Nome = u.Nome,
                    Email = u.Email,
                    Perfil = u.Perfil.Nome,
                    FotoUrl = u.FotoUrl,
                    Endereco = u.Endereco,
                    Latitude = u.Latitude,
                    Longitude = u.Longitude
                })
                .ToListAsync();

            return usuarios;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResumoDTO>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .Where(u => u.IDUsuario == id)
                .Select(u => new UsuarioResumoDTO
                {
                    IDUsuario = u.IDUsuario,
                    Nome = u.Nome,
                    Email = u.Email,
                    Perfil = u.Perfil.Nome,
                    FotoUrl = u.FotoUrl,
                    Endereco = u.Endereco,
                    Latitude = u.Latitude,
                    Longitude = u.Longitude
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound(new { message = $"Usuário com ID {id} não encontrado." });
            }

            return usuario;
        }

        // Listagem administrativa: inclui usuários inativos e os campos de gestão
        // (perfil, status, datas). Ainda nunca retorna SenhaHash.
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<UsuarioAdminDTO>>> GetUsuariosAdmin()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Perfil)
                .OrderBy(u => u.Nome)
                .Select(u => new UsuarioAdminDTO
                {
                    IDUsuario = u.IDUsuario,
                    Nome = u.Nome,
                    Email = u.Email,
                    Login = u.Login,
                    IDPerfil = u.IDPerfil,
                    Perfil = u.Perfil.Nome,
                    Ativo = u.Ativo,
                    DataCriacao = u.DataCriacao,
                    UltimoAcesso = u.UltimoAcesso,
                    FotoUrl = u.FotoUrl,
                    Endereco = u.Endereco,
                    Latitude = u.Latitude,
                    Longitude = u.Longitude
                })
                .ToListAsync();

            return usuarios;
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioAdminDTO>> PostUsuario(CriarUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Usuarios.AnyAsync(u => u.Login.ToLower() == dto.Login.ToLower()))
            {
                return Conflict(new { message = "Já existe um usuário com este login." });
            }

            if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            {
                return Conflict(new { message = "Já existe um usuário com este e-mail." });
            }

            var perfil = await _context.Perfis.FindAsync(dto.IDPerfil);
            if (perfil == null)
            {
                return BadRequest(new { message = "Perfil inválido." });
            }

            if (perfil.ExigeEndereco && string.IsNullOrWhiteSpace(dto.Endereco))
            {
                return BadRequest(new { message = "Endereço é obrigatório para o perfil selecionado." });
            }

            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Login = dto.Login,
                SenhaHash = PasswordHasher.Hash(dto.SenhaInicial),
                IDPerfil = dto.IDPerfil,
                Ativo = true,
                DataCriacao = DateTime.Now,
                FotoUrl = dto.FotoUrl,
                Endereco = dto.Endereco,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IDUsuario }, new UsuarioAdminDTO
            {
                IDUsuario = usuario.IDUsuario,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Login = usuario.Login,
                IDPerfil = usuario.IDPerfil,
                Perfil = perfil.Nome,
                Ativo = usuario.Ativo,
                DataCriacao = usuario.DataCriacao,
                UltimoAcesso = usuario.UltimoAcesso,
                FotoUrl = usuario.FotoUrl,
                Endereco = usuario.Endereco,
                Latitude = usuario.Latitude,
                Longitude = usuario.Longitude
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, AtualizarUsuarioDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound(new { message = $"Usuário com ID {id} não encontrado." });
            }

            if (await _context.Usuarios.AnyAsync(u => u.IDUsuario != id && u.Login.ToLower() == dto.Login.ToLower()))
            {
                return Conflict(new { message = "Já existe um usuário com este login." });
            }

            if (await _context.Usuarios.AnyAsync(u => u.IDUsuario != id && u.Email.ToLower() == dto.Email.ToLower()))
            {
                return Conflict(new { message = "Já existe um usuário com este e-mail." });
            }

            var perfil = await _context.Perfis.FindAsync(dto.IDPerfil);
            if (perfil == null)
            {
                return BadRequest(new { message = "Perfil inválido." });
            }

            if (perfil.ExigeEndereco && string.IsNullOrWhiteSpace(dto.Endereco))
            {
                return BadRequest(new { message = "Endereço é obrigatório para o perfil selecionado." });
            }

            usuario.Nome = dto.Nome;
            usuario.Email = dto.Email;
            usuario.Login = dto.Login;
            usuario.IDPerfil = dto.IDPerfil;
            usuario.Ativo = dto.Ativo;
            usuario.FotoUrl = dto.FotoUrl;
            usuario.Endereco = dto.Endereco;
            usuario.Latitude = dto.Latitude;
            usuario.Longitude = dto.Longitude;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Autoatendimento: o próprio usuário logado edita seu perfil. Foto sempre pode ser
        // trocada; Nome/E-mail/Login/Perfil só são aplicados se o usuário já tiver permissão
        // de gerenciar usuários (ex.: Gestor) — verificado aqui no servidor, não confia em
        // nenhum controle de UI, já que os demais controllers deste projeto não usam [Authorize]
        // nem barram edição por permissão.
        [Authorize]
        [HttpPut("meu-perfil")]
        public async Task<ActionResult<UsuarioInfo>> AtualizarMeuPerfil(AtualizarMeuPerfilDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var idUsuarioLogado))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.IDUsuario == idUsuarioLogado);
            if (usuario == null)
            {
                return Unauthorized();
            }

            if (dto.FotoUrl != null)
            {
                usuario.FotoUrl = dto.FotoUrl;
            }

            // Endereço qualquer usuário pode manter atualizado (igual à foto) — não é um
            // campo sensível como Nome/E-mail/Login/Perfil, que só Gestor edita.
            if (dto.Endereco != null)
            {
                usuario.Endereco = dto.Endereco;
                usuario.Latitude = dto.Latitude;
                usuario.Longitude = dto.Longitude;
            }

            var exigeEnderecoFinal = usuario.Perfil.ExigeEndereco;

            if (usuario.Perfil.PodeGerenciarUsuarios)
            {
                if (!string.IsNullOrWhiteSpace(dto.Nome))
                {
                    usuario.Nome = dto.Nome;
                }

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    usuario.Email = dto.Email;
                }

                if (!string.IsNullOrWhiteSpace(dto.Login))
                {
                    usuario.Login = dto.Login;
                }

                if (dto.IDPerfil.HasValue)
                {
                    var perfilAlvo = await _context.Perfis.FindAsync(dto.IDPerfil.Value);
                    if (perfilAlvo != null)
                    {
                        usuario.IDPerfil = perfilAlvo.IDPerfil;
                        exigeEnderecoFinal = perfilAlvo.ExigeEndereco;
                    }
                }
            }

            // Baseado no perfil que vai valer depois deste salvamento (já considerando uma
            // eventual troca de perfil pelo Gestor acima), não no perfil antigo.
            if (exigeEnderecoFinal && string.IsNullOrWhiteSpace(usuario.Endereco))
            {
                return BadRequest(new { message = "Endereço é obrigatório para o perfil selecionado." });
            }

            await _context.SaveChangesAsync();

            // Recarrega o Perfil caso o IDPerfil tenha mudado acima, para o token/UsuarioInfo
            // retornado refletir o perfil novo (CreateUsuarioInfo lê usuario.Perfil.Nome).
            await _context.Entry(usuario).Reference(u => u.Perfil).LoadAsync();

            return _jwtService.CreateUsuarioInfo(usuario);
        }

        [HttpPost("{id}/resetar-senha")]
        public async Task<IActionResult> ResetarSenha(int id, ResetarSenhaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound(new { message = $"Usuário com ID {id} não encontrado." });
            }

            usuario.SenhaHash = PasswordHasher.Hash(dto.NovaSenha);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class UsuarioResumoDTO
    {
        public int IDUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
        // Usados pelo mapa de Gestão de Rota (ver GestaoPiscina.Client/Pages/Rotas.razor).
        public string? FotoUrl { get; set; }
        public string? Endereco { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class UsuarioAdminDTO
    {
        public int IDUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public int IDPerfil { get; set; }
        public string Perfil { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? UltimoAcesso { get; set; }
        public string? FotoUrl { get; set; }
        public string? Endereco { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class CriarUsuarioDTO
    {
        [Required] [StringLength(150)] public string Nome { get; set; } = string.Empty;
        [Required] [StringLength(100)] [EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] [StringLength(20)] public string Login { get; set; } = string.Empty;
        [Required] [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")] public string SenhaInicial { get; set; } = string.Empty;
        [Required] public int IDPerfil { get; set; }
        [StringLength(500)] public string? FotoUrl { get; set; }
        [StringLength(300)] public string? Endereco { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class AtualizarUsuarioDTO
    {
        [Required] [StringLength(150)] public string Nome { get; set; } = string.Empty;
        [Required] [StringLength(100)] [EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] [StringLength(20)] public string Login { get; set; } = string.Empty;
        [Required] public int IDPerfil { get; set; }
        public bool Ativo { get; set; } = true;
        [StringLength(500)] public string? FotoUrl { get; set; }
        [StringLength(300)] public string? Endereco { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    // Autoatendimento (ver AtualizarMeuPerfil): todos os campos são opcionais — Nome/Email/
    // Login/IDPerfil são simplesmente ignorados no servidor se o usuário não tiver permissão
    // de gerenciar usuários, em vez de exigir que o cliente monte um payload diferente por caso.
    public class AtualizarMeuPerfilDTO
    {
        [StringLength(150)] public string? Nome { get; set; }
        [StringLength(100)] [EmailAddress] public string? Email { get; set; }
        [StringLength(20)] public string? Login { get; set; }
        public int? IDPerfil { get; set; }
        [StringLength(500)] public string? FotoUrl { get; set; }
        [StringLength(300)] public string? Endereco { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class ResetarSenhaDTO
    {
        [Required] [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")] public string NovaSenha { get; set; } = string.Empty;
    }
}
