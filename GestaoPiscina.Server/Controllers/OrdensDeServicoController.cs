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
            var erroValidacao = ValidarRegrasDeNegocio(ordemDeServico);
            if (erroValidacao != null)
            {
                return BadRequest(new { message = erroValidacao });
            }

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

            var erroValidacao = ValidarRegrasDeNegocio(ordemDeServico);
            if (erroValidacao != null)
            {
                return BadRequest(new { message = erroValidacao });
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

                var tecnicoPadrao = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Perfil.Nome == "Técnico" && u.Ativo);
                if (tecnicoPadrao == null)
                {
                    return BadRequest(new { message = "Nenhum técnico cadastrado para atribuir às OS automáticas." });
                }

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
                        // Campos de medição/execução (pH, horários, etc.) ficam com valores neutros
                        // e são preenchidos pelo técnico quando a OS é executada/finalizada.
                        var novaOS = new OrdemDeServico
                        {
                            IDPiscina = piscina.IDPiscina,
                            IDUsuario = tecnicoPadrao.IDUsuario,
                            DataExecucao = hoje,
                            Status = "Em Aberto",
                            ChecklistConcluido = false,
                            RelatorioGerado = false,
                            HoraInicio = hoje,
                            HoraTermino = hoje
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

        // Validações leves de negócio (RN01 e regra de aprovador para Ocorrência).
        // Não substitui um motor de regras completo — cobre só o mínimo para não persistir dados inconsistentes.
        private static string? ValidarRegrasDeNegocio(OrdemDeServico ordemDeServico)
        {
            if (ordemDeServico.Status == "Finalizada" && !ordemDeServico.ChecklistConcluido)
            {
                return "A OS só pode ser finalizada com o checklist obrigatório concluído.";
            }

            if (ordemDeServico.Status == "Ocorrência" && ordemDeServico.Aprovador == null)
            {
                return "É necessário informar o aprovador responsável para finalizar uma OS com ocorrência.";
            }

            return null;
        }

        private bool OrdemDeServicoExists(int id)
        {
            return _context.OrdensDeServico.Any(e => e.IDOS == id);
        }
    }
} 