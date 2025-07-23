using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipamentosController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public EquipamentosController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        // GET: api/Equipamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipamento>>> GetEquipamentos()
        {
            return await _context.Equipamentos
                .Include(e => e.Cliente)
                .ToListAsync();
        }

        // GET: api/Equipamentos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipamento>> GetEquipamento(int id)
        {
            var equipamento = await _context.Equipamentos
                .Include(e => e.Cliente)
                .FirstOrDefaultAsync(e => e.IDEquipamento == id);

            if (equipamento == null)
            {
                return NotFound();
            }

            return equipamento;
        }

        // GET: api/Equipamentos/cliente/5
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Equipamento>>> GetEquipamentosByCliente(int clienteId)
        {
            return await _context.Equipamentos
                .Where(e => e.IDCliente == clienteId)
                .ToListAsync();
        }

        // POST: api/Equipamentos
        [HttpPost]
        public async Task<ActionResult<Equipamento>> CreateEquipamento(Equipamento equipamento)
        {
            // Verificar se o cliente existe
            var cliente = await _context.Clientes.FindAsync(equipamento.IDCliente);
            if (cliente == null)
            {
                return BadRequest("Cliente não encontrado.");
            }

            _context.Equipamentos.Add(equipamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEquipamento), new { id = equipamento.IDEquipamento }, equipamento);
        }

        // PUT: api/Equipamentos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEquipamento(int id, Equipamento equipamento)
        {
            if (id != equipamento.IDEquipamento)
            {
                return BadRequest();
            }

            // Verificar se o cliente existe
            var cliente = await _context.Clientes.FindAsync(equipamento.IDCliente);
            if (cliente == null)
            {
                return BadRequest("Cliente não encontrado.");
            }

            _context.Entry(equipamento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipamentoExists(id))
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

        // DELETE: api/Equipamentos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipamento(int id)
        {
            var equipamento = await _context.Equipamentos.FindAsync(id);
            if (equipamento == null)
            {
                return NotFound();
            }

            _context.Equipamentos.Remove(equipamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EquipamentoExists(int id)
        {
            return _context.Equipamentos.Any(e => e.IDEquipamento == id);
        }
    }
} 