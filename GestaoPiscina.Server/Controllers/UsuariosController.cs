using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;

namespace GestaoPiscina.Server.Controllers
{
    // Somente leitura: usado para popular seletores de usuário (ex: técnico/aprovador na OS).
    // Nunca retorna SenhaHash.
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public UsuariosController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResumoDTO>>> GetUsuarios([FromQuery] string? perfil)
        {
            var query = _context.Usuarios.Include(u => u.Perfil).Where(u => u.Ativo);

            if (!string.IsNullOrWhiteSpace(perfil))
            {
                query = query.Where(u => u.Perfil.Nome == perfil);
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
    }

    public class UsuarioResumoDTO
    {
        public int IDUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
    }
}
