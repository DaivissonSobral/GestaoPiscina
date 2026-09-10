using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GestoresController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public GestoresController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gestor>>> GetGestores()
        {
            return await _context.Gestores
                .Include(g => g.GestorClientes)
                    .ThenInclude(gc => gc.Cliente)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Gestor>> GetGestor(int id)
        {
            var gestor = await _context.Gestores
                .Include(g => g.GestorClientes)
                    .ThenInclude(gc => gc.Cliente)
                .FirstOrDefaultAsync(g => g.IDGestor == id);

            if (gestor == null)
            {
                return NotFound(new { message = $"Gestor com ID {id} não encontrado." });
            }

            return gestor;
        }

        [HttpPost]
        public async Task<ActionResult<Gestor>> PostGestor(Gestor gestor)
        {
            if (await _context.Gestores.AnyAsync(g => g.Email.ToLower() == gestor.Email.ToLower()))
            {
                return Conflict(new { message = "Já existe um gestor com este e-mail." });
            }

            _context.Gestores.Add(gestor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGestor), new { id = gestor.IDGestor }, gestor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutGestor(int id, Gestor gestor)
        {
            if (id != gestor.IDGestor)
            {
                return BadRequest(new { message = "ID na URL não corresponde ao ID do gestor." });
            }

            if (await _context.Gestores.AnyAsync(g => g.Email.ToLower() == gestor.Email.ToLower() && g.IDGestor != id))
            {
                return Conflict(new { message = "Já existe um gestor com este e-mail." });
            }

            _context.Entry(gestor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GestorExists(id))
                {
                    return NotFound(new { message = $"Gestor com ID {id} não encontrado." });
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGestor(int id)
        {
            var gestor = await _context.Gestores.FindAsync(id);
            if (gestor == null)
            {
                return NotFound(new { message = $"Gestor com ID {id} não encontrado." });
            }

            _context.Gestores.Remove(gestor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/gestores/{id}/clientes
        [HttpGet("{id}/clientes")]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientesDoGestor(int id)
        {
            var gestor = await _context.Gestores.FindAsync(id);
            if (gestor == null)
            {
                return NotFound(new { message = $"Gestor com ID {id} não encontrado." });
            }

            var clientes = await _context.GestorClientes
                .Where(gc => gc.IDGestor == id)
                .Include(gc => gc.Cliente)
                .Select(gc => gc.Cliente)
                .ToListAsync();

            return clientes;
        }

        // POST: api/gestores/{id}/clientes/{clienteId}
        [HttpPost("{id}/clientes/{clienteId}")]
        public async Task<IActionResult> VincularCliente(int id, int clienteId)
        {
            var gestor = await _context.Gestores.FindAsync(id);
            if (gestor == null)
            {
                return NotFound(new { message = $"Gestor com ID {id} não encontrado." });
            }

            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                return NotFound(new { message = $"Cliente com ID {clienteId} não encontrado." });
            }

            var vinculoExistente = await _context.GestorClientes
                .FirstOrDefaultAsync(gc => gc.IDGestor == id && gc.IDCliente == clienteId);
            if (vinculoExistente != null)
            {
                return Conflict(new { message = "Este gestor já está vinculado a este cliente." });
            }

            _context.GestorClientes.Add(new GestorCliente { IDGestor = id, IDCliente = clienteId });
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/gestores/{id}/clientes/{clienteId}
        [HttpDelete("{id}/clientes/{clienteId}")]
        public async Task<IActionResult> DesvincularCliente(int id, int clienteId)
        {
            var vinculo = await _context.GestorClientes
                .FirstOrDefaultAsync(gc => gc.IDGestor == id && gc.IDCliente == clienteId);
            if (vinculo == null)
            {
                return NotFound(new { message = "Vínculo não encontrado." });
            }

            _context.GestorClientes.Remove(vinculo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GestorExists(int id)
        {
            return _context.Gestores.Any(g => g.IDGestor == id);
        }
    }
}
