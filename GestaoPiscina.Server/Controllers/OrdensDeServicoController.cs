using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdensDeServicoController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;
        private readonly IPushNotificationService _pushNotificationService;

        public OrdensDeServicoController(GestaoPiscinaContext context, IPushNotificationService pushNotificationService)
        {
            _context = context;
            _pushNotificationService = pushNotificationService;
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
            var erroValidacao = await ValidarRegrasDeNegocioAsync(ordemDeServico);
            if (erroValidacao != null)
            {
                return BadRequest(new { message = erroValidacao });
            }

            _context.OrdensDeServico.Add(ordemDeServico);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Não foi possível salvar a Ordem de Serviço. Verifique os dados informados." });
            }

            await NotificarOcorrenciaSeNecessarioAsync(ordemDeServico);

            return CreatedAtAction(nameof(GetOrdemDeServico), new { id = ordemDeServico.IDOS }, ordemDeServico);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrdemDeServico(int id, OrdemDeServico ordemDeServico)
        {
            if (id != ordemDeServico.IDOS)
            {
                return BadRequest();
            }

            // Uma OS já persistida com ocorrência não pode mais ser alterada por aqui — a única
            // mutação permitida depois disso é a aprovação, feita pelo endpoint próprio abaixo.
            var statusAtual = await _context.OrdensDeServico
                .AsNoTracking()
                .Where(o => o.IDOS == id)
                .Select(o => o.Status)
                .FirstOrDefaultAsync();

            if (statusAtual == null)
            {
                return NotFound();
            }

            if (statusAtual == "Ocorrência")
            {
                return BadRequest(new { message = "Uma OS com ocorrência registrada não pode mais ser alterada." });
            }

            var erroValidacao = await ValidarRegrasDeNegocioAsync(ordemDeServico);
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
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Não foi possível salvar a Ordem de Serviço. Verifique os dados informados." });
            }

            await NotificarOcorrenciaSeNecessarioAsync(ordemDeServico);

            return NoContent();
        }

        // Notifica o químico responsável (Aprovador) via push quando a OS é salva com uma
        // ocorrência recém-registrada. Nunca deixa uma falha no envio derrubar o salvamento.
        private async Task NotificarOcorrenciaSeNecessarioAsync(OrdemDeServico ordemDeServico)
        {
            if (ordemDeServico.Status != "Ocorrência" || ordemDeServico.Aprovador == null)
            {
                return;
            }

            try
            {
                var piscina = await _context.Piscinas
                    .Include(p => p.Cliente)
                    .FirstOrDefaultAsync(p => p.IDPiscina == ordemDeServico.IDPiscina);
                var nomeCliente = piscina?.Cliente?.Nome ?? "cliente";

                await _pushNotificationService.EnviarParaUsuarioAsync(
                    ordemDeServico.Aprovador.Value,
                    "Nova Ocorrência",
                    $"Uma ocorrência foi registrada para {nomeCliente} e aguarda sua aprovação.",
                    $"/ordens-servico?os={ordemDeServico.IDOS}");
            }
            catch
            {
                // Falha no push nunca deve impedir o salvamento da OS, que já aconteceu.
            }
        }

        [Authorize]
        [HttpPatch("{id}/aprovar-ocorrencia")]
        public async Task<IActionResult> AprovarOcorrencia(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var idUsuarioLogado))
            {
                return Unauthorized();
            }

            var ordemDeServico = await _context.OrdensDeServico.FindAsync(id);
            if (ordemDeServico == null)
            {
                return NotFound();
            }

            if (ordemDeServico.Status != "Ocorrência")
            {
                return BadRequest(new { message = "Só é possível aprovar uma OS com ocorrência registrada." });
            }

            if (ordemDeServico.Aprovador != idUsuarioLogado)
            {
                return Forbid();
            }

            if (ordemDeServico.OcorrenciaReprovada)
            {
                return BadRequest(new { message = "Esta ocorrência já foi reprovada." });
            }

            if (!ordemDeServico.OcorrenciaAprovada)
            {
                ordemDeServico.OcorrenciaAprovada = true;
                ordemDeServico.DataAprovacaoOcorrencia = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Ok(ordemDeServico);
        }

        [Authorize]
        [HttpPatch("{id}/reprovar-ocorrencia")]
        public async Task<IActionResult> ReprovarOcorrencia(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var idUsuarioLogado))
            {
                return Unauthorized();
            }

            var ordemDeServico = await _context.OrdensDeServico.FindAsync(id);
            if (ordemDeServico == null)
            {
                return NotFound();
            }

            if (ordemDeServico.Status != "Ocorrência")
            {
                return BadRequest(new { message = "Só é possível reprovar uma OS com ocorrência registrada." });
            }

            if (ordemDeServico.Aprovador != idUsuarioLogado)
            {
                return Forbid();
            }

            if (ordemDeServico.OcorrenciaAprovada)
            {
                return BadRequest(new { message = "Esta ocorrência já foi aprovada." });
            }

            if (!ordemDeServico.OcorrenciaReprovada)
            {
                ordemDeServico.OcorrenciaReprovada = true;
                ordemDeServico.DataReprovacaoOcorrencia = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Ok(ordemDeServico);
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
        public async Task<ActionResult<IEnumerable<OrdemDeServico>>> GerarOSAutomaticas([FromBody] GerarOSAutomaticasRequest? request)
        {
            try
            {
                var dataInicio = (request?.DataInicio ?? DateTime.Today).Date;
                var dataFim = (request?.DataFim ?? DateTime.Today).Date;

                if (dataFim < dataInicio)
                {
                    return BadRequest(new { message = "A data final deve ser maior ou igual à data inicial." });
                }

                if ((dataFim - dataInicio).TotalDays > 366)
                {
                    return BadRequest(new { message = "O período não pode ultrapassar 1 ano." });
                }

                var piscinas = await _context.Piscinas
                    .Include(p => p.Cliente)
                    .Where(p => p.RecorrenciaFrequencia != "Nenhuma")
                    .ToListAsync();

                // Datas já cobertas por OS existente, por piscina (evita duplicar).
                var existentes = await _context.OrdensDeServico
                    .Where(o => o.DataExecucao.Date >= dataInicio && o.DataExecucao.Date <= dataFim)
                    .Select(o => new { o.IDPiscina, Data = o.DataExecucao.Date })
                    .ToListAsync();
                var datasExistentes = existentes.Select(x => (x.IDPiscina, x.Data)).ToHashSet();

                // Quantas ocorrências cada piscina já teve ANTES do período pedido — para
                // respeitar corretamente o limite de "Termina após N ocorrências" mesmo
                // gerando várias datas de uma vez (o contador é incrementado em memória
                // conforme cada OS é adicionada abaixo, sem re-consultar o banco a cada dia).
                var ocorrenciasGeradas = new Dictionary<int, int>();
                foreach (var piscina in piscinas.Where(p => p.RecorrenciaTermino == "Ocorrencias" && p.RecorrenciaOcorrencias.HasValue))
                {
                    ocorrenciasGeradas[piscina.IDPiscina] = await _context.OrdensDeServico
                        .CountAsync(o => o.IDPiscina == piscina.IDPiscina
                            && o.DataExecucao.Date >= piscina.RecorrenciaDataInicio!.Value.Date
                            && o.DataExecucao.Date < dataInicio);
                }

                var osCriadas = new List<OrdemDeServico>();

                for (var data = dataInicio; data <= dataFim; data = data.AddDays(1))
                {
                    foreach (var piscina in piscinas)
                    {
                        if (!DeveGerarOSHoje(piscina, data))
                        {
                            continue;
                        }

                        if (piscina.RecorrenciaTermino == "Ocorrencias" && piscina.RecorrenciaOcorrencias.HasValue
                            && ocorrenciasGeradas.GetValueOrDefault(piscina.IDPiscina) >= piscina.RecorrenciaOcorrencias.Value)
                        {
                            continue;
                        }

                        if (datasExistentes.Contains((piscina.IDPiscina, data)))
                        {
                            continue;
                        }

                        // Campos de medição/execução (pH, horários, etc.) ficam com valores neutros
                        // e são preenchidos pelo técnico quando a OS é executada/finalizada.
                        var novaOS = new OrdemDeServico
                        {
                            IDPiscina = piscina.IDPiscina,
                            IDUsuario = null,
                            DataExecucao = data,
                            Status = "Em Aberto",
                            ChecklistConcluido = false,
                            RelatorioGerado = false,
                            HoraInicio = default,
                            HoraTermino = default
                        };

                        _context.OrdensDeServico.Add(novaOS);
                        osCriadas.Add(novaOS);
                        datasExistentes.Add((piscina.IDPiscina, data));

                        if (piscina.RecorrenciaTermino == "Ocorrencias" && piscina.RecorrenciaOcorrencias.HasValue)
                        {
                            ocorrenciasGeradas[piscina.IDPiscina] = ocorrenciasGeradas.GetValueOrDefault(piscina.IDPiscina) + 1;
                        }
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

        // Verifica se, segundo a recorrência configurada na piscina, hoje é um dia de manutenção agendado.
        // Não avalia a condição de término por número de ocorrências (feita à parte, pois depende do banco).
        private static bool DeveGerarOSHoje(Piscina piscina, DateTime hoje)
        {
            var inicio = piscina.RecorrenciaDataInicio?.Date;
            if (inicio == null || hoje < inicio)
            {
                return false;
            }

            if (piscina.RecorrenciaTermino == "Data" && piscina.RecorrenciaDataFim.HasValue && hoje > piscina.RecorrenciaDataFim.Value.Date)
            {
                return false;
            }

            var intervalo = Math.Max(1, piscina.RecorrenciaIntervalo);

            switch (piscina.RecorrenciaFrequencia)
            {
                case "Diaria":
                    var dias = (hoje - inicio.Value).Days;
                    return dias % intervalo == 0;

                case "Semanal":
                    var diasSemana = (piscina.RecorrenciaDiasSemana ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (!diasSemana.Contains(DiaSemanaAbreviado(hoje.DayOfWeek)))
                    {
                        return false;
                    }

                    var semanasDesdeInicio = (InicioDaSemana(hoje) - InicioDaSemana(inicio.Value)).Days / 7;
                    return semanasDesdeInicio % intervalo == 0;

                case "Mensal":
                    if (hoje.Day != inicio.Value.Day)
                    {
                        return false;
                    }

                    var mesesDesdeInicio = (hoje.Year - inicio.Value.Year) * 12 + (hoje.Month - inicio.Value.Month);
                    return mesesDesdeInicio % intervalo == 0;

                default:
                    return false;
            }
        }

        private static DateTime InicioDaSemana(DateTime data)
        {
            return data.Date.AddDays(-(int)data.DayOfWeek);
        }

        private static string DiaSemanaAbreviado(DayOfWeek dia) => dia switch
        {
            DayOfWeek.Sunday => "Dom",
            DayOfWeek.Monday => "Seg",
            DayOfWeek.Tuesday => "Ter",
            DayOfWeek.Wednesday => "Qua",
            DayOfWeek.Thursday => "Qui",
            DayOfWeek.Friday => "Sex",
            DayOfWeek.Saturday => "Sab",
            _ => string.Empty
        };

        // Validações leves de negócio (RN01, regra de aprovador para Ocorrência, e
        // integridade referencial/temporal mínima para não persistir dados inconsistentes).
        // Não substitui um motor de regras completo.
        private async Task<string?> ValidarRegrasDeNegocioAsync(OrdemDeServico ordemDeServico)
        {
            if (ordemDeServico.Status == "Finalizada" && !ordemDeServico.ChecklistConcluido)
            {
                return "A OS só pode ser finalizada com o checklist obrigatório concluído.";
            }

            if (ordemDeServico.Status == "Ocorrência" && ordemDeServico.Aprovador == null)
            {
                return "É necessário informar o aprovador responsável para finalizar uma OS com ocorrência.";
            }

            if (ordemDeServico.Status == "Cancelada" && string.IsNullOrWhiteSpace(ordemDeServico.Observacoes))
            {
                return "Observações são obrigatórias para cancelar a OS.";
            }

            // Fora de Finalizada/Ocorrência, o horário de término não é editável no formulário
            // e pode carregar um valor antigo (ex.: meia-noite) — não validar contra ele aqui.
            if (ordemDeServico.Status is "Finalizada" or "Ocorrência" && ordemDeServico.HoraTermino < ordemDeServico.HoraInicio)
            {
                return "O horário de término não pode ser anterior ao horário de início.";
            }

            if (!await _context.Piscinas.AnyAsync(p => p.IDPiscina == ordemDeServico.IDPiscina))
            {
                return "Selecione uma piscina válida.";
            }

            if (ordemDeServico.IDUsuario.HasValue
                && !await _context.Usuarios.AnyAsync(u => u.IDUsuario == ordemDeServico.IDUsuario.Value))
            {
                return "Selecione um técnico válido.";
            }

            if (ordemDeServico.Aprovador.HasValue
                && !await _context.Usuarios.AnyAsync(u => u.IDUsuario == ordemDeServico.Aprovador.Value))
            {
                return "Selecione um aprovador válido.";
            }

            return null;
        }

        private bool OrdemDeServicoExists(int id)
        {
            return _context.OrdensDeServico.Any(e => e.IDOS == id);
        }
    }

    public class GerarOSAutomaticasRequest
    {
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
} 