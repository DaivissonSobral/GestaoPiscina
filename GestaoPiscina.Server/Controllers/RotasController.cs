using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Controllers
{
    // Rotas confirmadas por técnico/dia (ver GestaoPiscina.Client/Pages/Rotas.razor) — só a
    // sequência de visita decidida; o técnico responsável por cada OS continua sendo
    // OrdemDeServico.IDUsuario (atualizado à parte, pelo próprio cliente, via
    // OrdensDeServicoController). "Concluída" é derivada comparando com o status das OS do
    // cliente na mesma data, nunca guardada aqui, pra não duplicar/dessincronizar estado.
    [ApiController]
    [Route("api/[controller]")]
    public class RotasController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public RotasController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        public class VisitaDTO
        {
            public int IDCliente { get; set; }
            public string NomeCliente { get; set; } = string.Empty;
            public int Ordem { get; set; }
            public bool Concluida { get; set; }
        }

        public class RotaTecnicoDTO
        {
            public int IDUsuario { get; set; }
            public DateTime Data { get; set; }
            public DateTime DataConfirmacao { get; set; }
            public List<VisitaDTO> Visitas { get; set; } = new();
        }

        public class SalvarRotaDTO
        {
            public int IDUsuario { get; set; }
            public DateTime Data { get; set; }
            public List<int> ClientesIds { get; set; } = new();
        }

        // GET api/rotas?data=2026-09-15 — rotas confirmadas de todos os técnicos nessa data.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RotaTecnicoDTO>>> GetRotas([FromQuery] DateTime data)
        {
            return await MontarRotasAsync(data, null);
        }

        // GET api/rotas/tecnico/8?data=2026-09-15 — rota confirmada de um técnico nessa data
        // (null se ele ainda não tem rota confirmada pra essa data).
        [HttpGet("tecnico/{idUsuario}")]
        public async Task<ActionResult<RotaTecnicoDTO?>> GetRotaTecnico(int idUsuario, [FromQuery] DateTime data)
        {
            var rotas = await MontarRotasAsync(data, idUsuario);
            return rotas.FirstOrDefault();
        }

        // PUT api/rotas — substitui (apaga e recria) a rota confirmada desse técnico nessa
        // data pela sequência informada. Chamado por "Confirmar atribuição" e por "Editar"
        // seguido de nova confirmação.
        [HttpPut]
        public async Task<ActionResult<RotaTecnicoDTO>> SalvarRota(SalvarRotaDTO dto)
        {
            if (dto.ClientesIds == null || dto.ClientesIds.Count == 0)
            {
                return BadRequest(new { message = "Informe ao menos um cliente na rota." });
            }

            if (!await _context.Usuarios.AnyAsync(u => u.IDUsuario == dto.IDUsuario))
            {
                return BadRequest(new { message = "Técnico inválido." });
            }

            var data = dto.Data.Date;
            var existentes = await _context.RotaVisitas
                .Where(r => r.IDUsuario == dto.IDUsuario && r.Data == data)
                .ToListAsync();
            _context.RotaVisitas.RemoveRange(existentes);

            var agora = DateTime.Now;
            var novas = dto.ClientesIds.Select((idCliente, indice) => new RotaVisita
            {
                IDUsuario = dto.IDUsuario,
                Data = data,
                IDCliente = idCliente,
                Ordem = indice + 1,
                DataConfirmacao = agora
            }).ToList();
            _context.RotaVisitas.AddRange(novas);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Não foi possível salvar a rota. Verifique os clientes informados." });
            }

            var rotas = await MontarRotasAsync(data, dto.IDUsuario);
            return rotas.First();
        }

        // DELETE api/rotas/tecnico/8?data=2026-09-15 — remove a rota confirmada (usado
        // quando o usuário clica em "Editar" pra ajustar antes de confirmar de novo).
        [HttpDelete("tecnico/{idUsuario}")]
        public async Task<IActionResult> ExcluirRota(int idUsuario, [FromQuery] DateTime data)
        {
            var existentes = await _context.RotaVisitas
                .Where(r => r.IDUsuario == idUsuario && r.Data == data.Date)
                .ToListAsync();
            _context.RotaVisitas.RemoveRange(existentes);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<List<RotaTecnicoDTO>> MontarRotasAsync(DateTime data, int? idUsuario)
        {
            data = data.Date;
            var query = _context.RotaVisitas
                .Include(r => r.Cliente)
                .Where(r => r.Data == data);
            if (idUsuario.HasValue)
            {
                query = query.Where(r => r.IDUsuario == idUsuario.Value);
            }

            var visitas = await query.ToListAsync();
            if (visitas.Count == 0)
            {
                return new List<RotaTecnicoDTO>();
            }

            var clientesIds = visitas.Select(v => v.IDCliente).Distinct().ToList();
            var statusPorCliente = await _context.OrdensDeServico
                .Where(o => o.DataExecucao.Date == data && clientesIds.Contains(o.Piscina.IDCliente) && o.Status != "Cancelada")
                .Select(o => new { o.Piscina.IDCliente, o.Status })
                .ToListAsync();

            bool ClienteConcluido(int idCliente)
            {
                var statusDoCliente = statusPorCliente.Where(s => s.IDCliente == idCliente).ToList();
                return statusDoCliente.Count > 0 && statusDoCliente.All(s => s.Status is "Finalizada" or "Ocorrência");
            }

            return visitas
                .GroupBy(v => v.IDUsuario)
                .Select(g => new RotaTecnicoDTO
                {
                    IDUsuario = g.Key,
                    Data = data,
                    DataConfirmacao = g.Max(v => v.DataConfirmacao),
                    Visitas = g.OrderBy(v => v.Ordem).Select(v => new VisitaDTO
                    {
                        IDCliente = v.IDCliente,
                        NomeCliente = v.Cliente.Nome,
                        Ordem = v.Ordem,
                        Concluida = ClienteConcluido(v.IDCliente)
                    }).ToList()
                })
                .ToList();
        }
    }
}
