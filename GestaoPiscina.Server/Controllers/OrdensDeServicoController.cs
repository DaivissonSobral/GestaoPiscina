using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdensDeServicoController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public OrdensDeServicoController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdemDeServico>>> GetOrdensDeServico()
        {
            return await _context.OrdensDeServico
                .Include(o => o.Piscina)
                .ThenInclude(p => p.Cliente)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrdemDeServico>> GetOrdemDeServico(int id)
        {
            var ordemDeServico = await _context.OrdensDeServico
                .Include(o => o.Piscina)
                .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(o => o.IDOS == id);

            if (ordemDeServico == null)
            {
                return NotFound();
            }

            return ordemDeServico;
        }

        [HttpGet("piscina/{piscinaId}")]
        public async Task<ActionResult<IEnumerable<OrdemDeServico>>> GetOrdensByPiscina(int piscinaId)
        {
            return await _context.OrdensDeServico
                .Include(o => o.Piscina)
                .ThenInclude(p => p.Cliente)
                .Where(o => o.IDPiscina == piscinaId)
                .OrderByDescending(o => o.DataExecucao)
                .ToListAsync();
        }

        [HttpGet("hoje")]
        public async Task<ActionResult<IEnumerable<OrdemDeServico>>> GetOrdensDeHoje()
        {
            var hoje = DateTime.Today;
            return await _context.OrdensDeServico
                .Include(o => o.Piscina)
                .ThenInclude(p => p.Cliente)
                .Where(o => o.DataExecucao.Date == hoje)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<OrdemDeServico>> PostOrdemDeServico(OrdemDeServico ordemDeServico)
        {
            _context.OrdensDeServico.Add(ordemDeServico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrdemDeServico), new { id = ordemDeServico.IDOS }, ordemDeServico);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrdemDeServico(int id, OrdemDeServico ordemDeServico)
        {
            if (id != ordemDeServico.IDOS)
            {
                return BadRequest();
            }

            _context.Entry(ordemDeServico).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrdemDeServicoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrdemDeServico(int id)
        {
            var ordemDeServico = await _context.OrdensDeServico.FindAsync(id);
            if (ordemDeServico == null)
            {
                return NotFound();
            }

            _context.OrdensDeServico.Remove(ordemDeServico);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("gerar-automaticas")]
        public async Task<ActionResult<IEnumerable<OrdemDeServico>>> GerarOSAutomaticas()
        {
            try
            {
                var hoje = DateTime.Today;
                var piscinas = await _context.Piscinas
                    .Include(p => p.Cliente)
                    .ToListAsync();

                var osCriadas = new List<OrdemDeServico>();

                foreach (var piscina in piscinas)
                {
                    var osExistente = await _context.OrdensDeServico
                        .FirstOrDefaultAsync(o => o.IDPiscina == piscina.IDPiscina && o.DataExecucao.Date == hoje);

                    if (osExistente == null)
                    {
                        var novaOS = new OrdemDeServico
                        {
                            IDPiscina = piscina.IDPiscina,
                            DataExecucao = hoje,
                            Status = "Em Aberto",
                            ChecklistConcluido = false,
                            RelatorioGerado = false
                        };

                        _context.OrdensDeServico.Add(novaOS);
                        osCriadas.Add(novaOS);
                    }
                }

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOrdensDeServico), osCriadas);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao gerar OS automáticas: {ex.Message}");
            }
        }

        private bool OrdemDeServicoExists(int id)
        {
            return _context.OrdensDeServico.Any(e => e.IDOS == id);
        }
    }
} 