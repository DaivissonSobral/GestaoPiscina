using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipamentosController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;
        private readonly ILogger<EquipamentosController> _logger;

        public EquipamentosController(GestaoPiscinaContext context, ILogger<EquipamentosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/equipamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipamento>>> GetEquipamentos()
        {
            try
            {
                _logger.LogInformation("Buscando todos os equipamentos");
                
                var equipamentos = await _context.Equipamentos
                    .Include(e => e.Cliente)
                    .ToListAsync();

                _logger.LogInformation($"Encontrados {equipamentos.Count} equipamentos");
                return Ok(equipamentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar equipamentos");
                return StatusCode(500, "Erro interno do servidor ao buscar equipamentos");
            }
        }

        // GET: api/equipamentos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipamento>> GetEquipamento(int id)
        {
            try
            {
                _logger.LogInformation($"Buscando equipamento com ID: {id}");

                if (id <= 0)
                {
                    _logger.LogWarning("ID de equipamento inválido: {Id}", id);
                    return BadRequest("ID de equipamento inválido");
                }

                var equipamento = await _context.Equipamentos
                    .Include(e => e.Cliente)
                    .FirstOrDefaultAsync(e => e.IDEquipamento == id);

                if (equipamento == null)
                {
                    _logger.LogWarning("Equipamento não encontrado com ID: {Id}", id);
                    return NotFound($"Equipamento com ID {id} não encontrado");
                }

                _logger.LogInformation($"Equipamento encontrado: {equipamento.IDEquipamento}");
                return Ok(equipamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar equipamento com ID: {Id}", id);
                return StatusCode(500, "Erro interno do servidor ao buscar equipamento");
            }
        }

        // GET: api/equipamentos/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Equipamento>>> GetEquipamentosByCliente(int clienteId)
        {
            try
            {
                _logger.LogInformation($"Buscando equipamentos do cliente: {clienteId}");

                if (clienteId <= 0)
                {
                    _logger.LogWarning("ID de cliente inválido: {ClienteId}", clienteId);
                    return BadRequest("ID de cliente inválido");
                }

                // Verificar se o cliente existe
                var cliente = await _context.Clientes.FindAsync(clienteId);
                if (cliente == null)
                {
                    _logger.LogWarning("Cliente não encontrado: {ClienteId}", clienteId);
                    return NotFound($"Cliente com ID {clienteId} não encontrado");
                }

                var equipamentos = await _context.Equipamentos
                    .Include(e => e.Cliente)
                    .Where(e => e.IDCliente == clienteId)
                    .ToListAsync();

                _logger.LogInformation($"Encontrados {equipamentos.Count} equipamentos para o cliente {clienteId}");
                return Ok(equipamentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar equipamentos do cliente: {ClienteId}", clienteId);
                return StatusCode(500, "Erro interno do servidor ao buscar equipamentos do cliente");
            }
        }

        // POST: api/equipamentos
        [HttpPost]
        public async Task<ActionResult<Equipamento>> CreateEquipamento([FromBody] Equipamento equipamento)
        {
            try
            {
                _logger.LogInformation("Tentando criar novo equipamento");

                // Validação do modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("Dados inválidos para criação de equipamento: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { message = "Dados inválidos", errors });
                }

                // Validações de negócio
                if (string.IsNullOrWhiteSpace(equipamento.Tipo))
                {
                    _logger.LogWarning("Tipo de equipamento não informado");
                    return BadRequest(new { message = "Tipo de equipamento é obrigatório" });
                }

                if (string.IsNullOrWhiteSpace(equipamento.NumeroSerie))
                {
                    _logger.LogWarning("Número de série não informado");
                    return BadRequest(new { message = "Número de série é obrigatório" });
                }

                // Verificar se o cliente existe
                var cliente = await _context.Clientes.FindAsync(equipamento.IDCliente);
                if (cliente == null)
                {
                    _logger.LogWarning("Cliente não encontrado: {ClienteId}", equipamento.IDCliente);
                    return BadRequest(new { message = "Cliente não encontrado" });
                }

                // Verificar se já existe um equipamento com o mesmo número de série
                var equipamentoExistente = await _context.Equipamentos
                    .FirstOrDefaultAsync(e => e.NumeroSerie.ToLower() == equipamento.NumeroSerie.ToLower());
                
                if (equipamentoExistente != null)
                {
                    _logger.LogWarning("Já existe um equipamento com o número de série {NumeroSerie}", equipamento.NumeroSerie);
                    return BadRequest(new { message = $"Já existe um equipamento com o número de série {equipamento.NumeroSerie}" });
                }

                // Garantir que o ID seja zero para novo equipamento
                equipamento.IDEquipamento = 0;

                _context.Equipamentos.Add(equipamento);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Equipamento criado com sucesso: {EquipamentoId}", equipamento.IDEquipamento);

                // Retornar o equipamento criado com dados do cliente
                var equipamentoCriado = await _context.Equipamentos
                    .Include(e => e.Cliente)
                    .FirstOrDefaultAsync(e => e.IDEquipamento == equipamento.IDEquipamento);

                return CreatedAtAction(nameof(GetEquipamento), new { id = equipamento.IDEquipamento }, equipamentoCriado);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao criar equipamento");
                return StatusCode(500, new { message = "Erro ao salvar equipamento no banco de dados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar equipamento");
                return StatusCode(500, new { message = "Erro interno do servidor ao criar equipamento" });
            }
        }

        // PUT: api/equipamentos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEquipamento(int id, [FromBody] Equipamento equipamento)
        {
            try
            {
                _logger.LogInformation("Tentando atualizar equipamento: {Id}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("ID de equipamento inválido: {Id}", id);
                    return BadRequest(new { message = "ID de equipamento inválido" });
                }

                if (id != equipamento.IDEquipamento)
                {
                    _logger.LogWarning("ID na URL não corresponde ao ID do equipamento");
                    return BadRequest(new { message = "ID na URL não corresponde ao ID do equipamento" });
                }

                // Validação do modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("Dados inválidos para atualização de equipamento: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { message = "Dados inválidos", errors });
                }

                // Validações de negócio
                if (string.IsNullOrWhiteSpace(equipamento.Tipo))
                {
                    _logger.LogWarning("Tipo de equipamento não informado");
                    return BadRequest(new { message = "Tipo de equipamento é obrigatório" });
                }

                if (string.IsNullOrWhiteSpace(equipamento.NumeroSerie))
                {
                    _logger.LogWarning("Número de série não informado");
                    return BadRequest(new { message = "Número de série é obrigatório" });
                }

                // Verificar se o equipamento existe
                var equipamentoExistente = await _context.Equipamentos.FindAsync(id);
                if (equipamentoExistente == null)
                {
                    _logger.LogWarning("Equipamento não encontrado: {Id}", id);
                    return NotFound(new { message = $"Equipamento com ID {id} não encontrado" });
                }

                // Verificar se o cliente existe
                var cliente = await _context.Clientes.FindAsync(equipamento.IDCliente);
                if (cliente == null)
                {
                    _logger.LogWarning("Cliente não encontrado: {ClienteId}", equipamento.IDCliente);
                    return BadRequest(new { message = "Cliente não encontrado" });
                }

                // Verificar se já existe outro equipamento com o mesmo número de série
                var outroEquipamento = await _context.Equipamentos
                    .FirstOrDefaultAsync(e => e.NumeroSerie.ToLower() == equipamento.NumeroSerie.ToLower() &&
                                            e.IDEquipamento != id);
                
                if (outroEquipamento != null)
                {
                    _logger.LogWarning("Já existe um equipamento com o número de série {NumeroSerie}", equipamento.NumeroSerie);
                    return BadRequest(new { message = $"Já existe um equipamento com o número de série {equipamento.NumeroSerie}" });
                }

                // Atualizar propriedades
                equipamentoExistente.Tipo = equipamento.Tipo;
                equipamentoExistente.NumeroSerie = equipamento.NumeroSerie;
                equipamentoExistente.UltimaCalibragem = equipamento.UltimaCalibragem;
                equipamentoExistente.Observacao = equipamento.Observacao;
                equipamentoExistente.IDCliente = equipamento.IDCliente;

                _context.Entry(equipamentoExistente).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Equipamento atualizado com sucesso: {Id}", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipamentoExists(id))
                {
                    _logger.LogWarning("Equipamento não encontrado durante atualização: {Id}", id);
                    return NotFound(new { message = $"Equipamento com ID {id} não encontrado" });
                }
                else
                {
                    _logger.LogError("Erro de concorrência ao atualizar equipamento: {Id}", id);
                    return StatusCode(500, new { message = "Erro de concorrência ao atualizar equipamento" });
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao atualizar equipamento: {Id}", id);
                return StatusCode(500, new { message = "Erro ao atualizar equipamento no banco de dados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar equipamento: {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor ao atualizar equipamento" });
            }
        }

        // DELETE: api/equipamentos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipamento(int id)
        {
            try
            {
                _logger.LogInformation("Tentando excluir equipamento: {Id}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("ID de equipamento inválido: {Id}", id);
                    return BadRequest(new { message = "ID de equipamento inválido" });
                }

                var equipamento = await _context.Equipamentos.FindAsync(id);

                if (equipamento == null)
                {
                    _logger.LogWarning("Equipamento não encontrado: {Id}", id);
                    return NotFound(new { message = $"Equipamento com ID {id} não encontrado" });
                }

                _context.Equipamentos.Remove(equipamento);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Equipamento excluído com sucesso: {Id}", id);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao excluir equipamento: {Id}", id);
                return StatusCode(500, new { message = "Erro ao excluir equipamento do banco de dados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao excluir equipamento: {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor ao excluir equipamento" });
            }
        }

        private bool EquipamentoExists(int id)
        {
            return _context.Equipamentos.Any(e => e.IDEquipamento == id);
        }
    }
} 
} 