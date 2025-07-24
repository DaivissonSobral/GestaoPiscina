using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PiscinasController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;
        private readonly ILogger<PiscinasController> _logger;

        public PiscinasController(GestaoPiscinaContext context, ILogger<PiscinasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/piscinas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Piscina>>> GetPiscinas()
        {
            try
            {
                _logger.LogInformation("Buscando todas as piscinas");
                
                var piscinas = await _context.Piscinas
                    .Include(p => p.Cliente)
                    .Include(p => p.OrdensDeServico)
                    .ToListAsync();

                _logger.LogInformation($"Encontradas {piscinas.Count} piscinas");
                return Ok(piscinas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar piscinas");
                return StatusCode(500, "Erro interno do servidor ao buscar piscinas");
            }
        }

        // GET: api/piscinas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Piscina>> GetPiscina(int id)
        {
            try
            {
                _logger.LogInformation($"Buscando piscina com ID: {id}");

                if (id <= 0)
                {
                    _logger.LogWarning("ID de piscina inválido: {Id}", id);
                    return BadRequest("ID de piscina inválido");
                }

                var piscina = await _context.Piscinas
                    .Include(p => p.Cliente)
                    .Include(p => p.OrdensDeServico)
                    .FirstOrDefaultAsync(p => p.IDPiscina == id);

                if (piscina == null)
                {
                    _logger.LogWarning("Piscina não encontrada com ID: {Id}", id);
                    return NotFound($"Piscina com ID {id} não encontrada");
                }

                _logger.LogInformation($"Piscina encontrada: {piscina.IDPiscina}");
                return Ok(piscina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar piscina com ID: {Id}", id);
                return StatusCode(500, "Erro interno do servidor ao buscar piscina");
            }
        }

        // GET: api/piscinas/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Piscina>>> GetPiscinasByCliente(int clienteId)
        {
            try
            {
                _logger.LogInformation($"Buscando piscinas do cliente: {clienteId}");

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

                var piscinas = await _context.Piscinas
                    .Include(p => p.Cliente)
                    .Include(p => p.OrdensDeServico)
                    .Where(p => p.IDCliente == clienteId)
                    .ToListAsync();

                _logger.LogInformation($"Encontradas {piscinas.Count} piscinas para o cliente {clienteId}");
                return Ok(piscinas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar piscinas do cliente: {ClienteId}", clienteId);
                return StatusCode(500, "Erro interno do servidor ao buscar piscinas do cliente");
            }
        }

        // POST: api/piscinas
        [HttpPost]
        public async Task<ActionResult<Piscina>> PostPiscina([FromBody] Piscina piscina)
        {
            try
            {
                _logger.LogInformation("Tentando criar nova piscina");

                // Validação do modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("Dados inválidos para criação de piscina: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { message = "Dados inválidos", errors });
                }

                // Validações de negócio
                if (string.IsNullOrWhiteSpace(piscina.Tipo))
                {
                    _logger.LogWarning("Tipo de piscina não informado");
                    return BadRequest(new { message = "Tipo de piscina é obrigatório" });
                }

                if (piscina.VolumeLitros <= 0)
                {
                    _logger.LogWarning("Volume de piscina deve ser maior que zero");
                    return BadRequest(new { message = "Volume de piscina deve ser maior que zero" });
                }

                // Verificar se o cliente existe
                var cliente = await _context.Clientes.FindAsync(piscina.IDCliente);
                if (cliente == null)
                {
                    _logger.LogWarning("Cliente não encontrado: {ClienteId}", piscina.IDCliente);
                    return BadRequest(new { message = "Cliente não encontrado" });
                }

                // Verificar se já existe uma piscina com o mesmo tipo para este cliente
                var piscinaExistente = await _context.Piscinas
                    .FirstOrDefaultAsync(p => p.IDCliente == piscina.IDCliente && 
                                            p.Tipo.ToLower() == piscina.Tipo.ToLower());
                
                if (piscinaExistente != null)
                {
                    _logger.LogWarning("Já existe uma piscina do tipo {Tipo} para o cliente {ClienteId}", 
                        piscina.Tipo, piscina.IDCliente);
                    return BadRequest(new { message = $"Já existe uma piscina do tipo {piscina.Tipo} para este cliente" });
                }

                // Garantir que o ID seja zero para nova piscina
                piscina.IDPiscina = 0;

                _context.Piscinas.Add(piscina);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Piscina criada com sucesso: {PiscinaId}", piscina.IDPiscina);

                // Retornar a piscina criada com dados do cliente
                var piscinaCriada = await _context.Piscinas
                    .Include(p => p.Cliente)
                    .FirstOrDefaultAsync(p => p.IDPiscina == piscina.IDPiscina);

                return CreatedAtAction(nameof(GetPiscina), new { id = piscina.IDPiscina }, piscinaCriada);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao criar piscina");
                return StatusCode(500, new { message = "Erro ao salvar piscina no banco de dados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar piscina");
                return StatusCode(500, new { message = "Erro interno do servidor ao criar piscina" });
            }
        }

        // PUT: api/piscinas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPiscina(int id, [FromBody] Piscina piscina)
        {
            try
            {
                _logger.LogInformation("Tentando atualizar piscina: {Id}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("ID de piscina inválido: {Id}", id);
                    return BadRequest(new { message = "ID de piscina inválido" });
                }

                if (id != piscina.IDPiscina)
                {
                    _logger.LogWarning("ID na URL não corresponde ao ID da piscina");
                    return BadRequest(new { message = "ID na URL não corresponde ao ID da piscina" });
                }

                // Validação do modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("Dados inválidos para atualização de piscina: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { message = "Dados inválidos", errors });
                }

                // Validações de negócio
                if (string.IsNullOrWhiteSpace(piscina.Tipo))
                {
                    _logger.LogWarning("Tipo de piscina não informado");
                    return BadRequest(new { message = "Tipo de piscina é obrigatório" });
                }

                if (piscina.VolumeLitros <= 0)
                {
                    _logger.LogWarning("Volume de piscina deve ser maior que zero");
                    return BadRequest(new { message = "Volume de piscina deve ser maior que zero" });
                }

                // Verificar se a piscina existe
                var piscinaExistente = await _context.Piscinas.FindAsync(id);
                if (piscinaExistente == null)
                {
                    _logger.LogWarning("Piscina não encontrada: {Id}", id);
                    return NotFound(new { message = $"Piscina com ID {id} não encontrada" });
                }

                // Verificar se o cliente existe
                var cliente = await _context.Clientes.FindAsync(piscina.IDCliente);
                if (cliente == null)
                {
                    _logger.LogWarning("Cliente não encontrado: {ClienteId}", piscina.IDCliente);
                    return BadRequest(new { message = "Cliente não encontrado" });
                }

                // Verificar se já existe outra piscina com o mesmo tipo para este cliente
                var outraPiscina = await _context.Piscinas
                    .FirstOrDefaultAsync(p => p.IDCliente == piscina.IDCliente && 
                                            p.Tipo.ToLower() == piscina.Tipo.ToLower() &&
                                            p.IDPiscina != id);
                
                if (outraPiscina != null)
                {
                    _logger.LogWarning("Já existe uma piscina do tipo {Tipo} para o cliente {ClienteId}", 
                        piscina.Tipo, piscina.IDCliente);
                    return BadRequest(new { message = $"Já existe uma piscina do tipo {piscina.Tipo} para este cliente" });
                }

                // Atualizar propriedades
                piscinaExistente.Tipo = piscina.Tipo;
                piscinaExistente.VolumeLitros = piscina.VolumeLitros;
                piscinaExistente.Localizacao = piscina.Localizacao;
                piscinaExistente.IDCliente = piscina.IDCliente;

                _context.Entry(piscinaExistente).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Piscina atualizada com sucesso: {Id}", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PiscinaExists(id))
                {
                    _logger.LogWarning("Piscina não encontrada durante atualização: {Id}", id);
                    return NotFound(new { message = $"Piscina com ID {id} não encontrada" });
                }
                else
                {
                    _logger.LogError("Erro de concorrência ao atualizar piscina: {Id}", id);
                    return StatusCode(500, new { message = "Erro de concorrência ao atualizar piscina" });
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao atualizar piscina: {Id}", id);
                return StatusCode(500, new { message = "Erro ao atualizar piscina no banco de dados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar piscina: {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor ao atualizar piscina" });
            }
        }

        // DELETE: api/piscinas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePiscina(int id)
        {
            try
            {
                _logger.LogInformation("Tentando excluir piscina: {Id}", id);

                if (id <= 0)
                {
                    _logger.LogWarning("ID de piscina inválido: {Id}", id);
                    return BadRequest(new { message = "ID de piscina inválido" });
                }

                var piscina = await _context.Piscinas
                    .Include(p => p.OrdensDeServico)
                    .FirstOrDefaultAsync(p => p.IDPiscina == id);

                if (piscina == null)
                {
                    _logger.LogWarning("Piscina não encontrada: {Id}", id);
                    return NotFound(new { message = $"Piscina com ID {id} não encontrada" });
                }

                // Verificar se há ordens de serviço associadas
                if (piscina.OrdensDeServico.Any())
                {
                    _logger.LogWarning("Não é possível excluir piscina com ordens de serviço: {Id}", id);
                    return BadRequest(new { message = "Não é possível excluir uma piscina que possui ordens de serviço associadas" });
                }

                _context.Piscinas.Remove(piscina);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Piscina excluída com sucesso: {Id}", id);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de banco de dados ao excluir piscina: {Id}", id);
                return StatusCode(500, new { message = "Erro ao excluir piscina do banco de dados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao excluir piscina: {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor ao excluir piscina" });
            }
        }

        private bool PiscinaExists(int id)
        {
            return _context.Piscinas.Any(e => e.IDPiscina == id);
        }
    }
} 