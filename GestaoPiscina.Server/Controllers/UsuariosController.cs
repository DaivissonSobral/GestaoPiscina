using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public UsuariosController(GestaoPiscinaContext context)
        {
            _context = context;
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
                    Perfil = u.Perfil.Nome
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
                    Perfil = u.Perfil.Nome
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
                    UltimoAcesso = u.UltimoAcesso
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

            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Login = dto.Login,
                SenhaHash = PasswordHasher.Hash(dto.SenhaInicial),
                IDPerfil = dto.IDPerfil,
                Ativo = true,
                DataCriacao = DateTime.Now
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
                UltimoAcesso = usuario.UltimoAcesso
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

            if (!await _context.Perfis.AnyAsync(p => p.IDPerfil == dto.IDPerfil))
            {
                return BadRequest(new { message = "Perfil inválido." });
            }

            usuario.Nome = dto.Nome;
            usuario.Email = dto.Email;
            usuario.Login = dto.Login;
            usuario.IDPerfil = dto.IDPerfil;
            usuario.Ativo = dto.Ativo;

            await _context.SaveChangesAsync();

            return NoContent();
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
    }

    public class CriarUsuarioDTO
    {
        [Required] [StringLength(150)] public string Nome { get; set; } = string.Empty;
        [Required] [StringLength(100)] [EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] [StringLength(20)] public string Login { get; set; } = string.Empty;
        [Required] [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")] public string SenhaInicial { get; set; } = string.Empty;
        [Required] public int IDPerfil { get; set; }
    }

    public class AtualizarUsuarioDTO
    {
        [Required] [StringLength(150)] public string Nome { get; set; } = string.Empty;
        [Required] [StringLength(100)] [EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] [StringLength(20)] public string Login { get; set; } = string.Empty;
        [Required] public int IDPerfil { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class ResetarSenhaDTO
    {
        [Required] [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")] public string NovaSenha { get; set; } = string.Empty;
    }
}
