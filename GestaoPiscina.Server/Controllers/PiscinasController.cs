using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PiscinasController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public PiscinasController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Piscina>>> GetPiscinas()
        {
            return await _context.Piscinas
                .Include(p => p.Cliente)
                .Include(p => p.OrdensDeServico)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Piscina>> GetPiscina(int id)
        {
            var piscina = await _context.Piscinas
                .Include(p => p.Cliente)
                .Include(p => p.OrdensDeServico)
                .FirstOrDefaultAsync(p => p.IDPiscina == id);

            if (piscina == null)
            {
                return NotFound();
            }

            return piscina;
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Piscina>>> GetPiscinasByCliente(int clienteId)
        {
            return await _context.Piscinas
                .Include(p => p.Cliente)
                .Include(p => p.OrdensDeServico)
                .Where(p => p.IDCliente == clienteId)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Piscina>> PostPiscina(Piscina piscina)
        {
            _context.Piscinas.Add(piscina);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPiscina), new { id = piscina.IDPiscina }, piscina);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPiscina(int id, Piscina piscina)
        {
            if (id != piscina.IDPiscina)
            {
                return BadRequest();
            }

            _context.Entry(piscina).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PiscinaExists(id))
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
        public async Task<IActionResult> DeletePiscina(int id)
        {
            var piscina = await _context.Piscinas.FindAsync(id);
            if (piscina == null)
            {
                return NotFound();
            }

            _context.Piscinas.Remove(piscina);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PiscinaExists(int id)
        {
            return _context.Piscinas.Any(e => e.IDPiscina == id);
        }
    }
} 