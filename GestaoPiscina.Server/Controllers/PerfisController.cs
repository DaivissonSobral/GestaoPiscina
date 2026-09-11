using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Controllers
{
    // Somente leitura: perfis são fixos nesta versão (sem tela de gestão de perfis).
    [ApiController]
    [Route("api/[controller]")]
    public class PerfisController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public PerfisController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Perfil>>> GetPerfis()
        {
            return await _context.Perfis
                .AsNoTracking()
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }
    }
}
