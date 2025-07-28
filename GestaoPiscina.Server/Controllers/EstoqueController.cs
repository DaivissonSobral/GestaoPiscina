using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstoqueController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public EstoqueController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<EstoqueCliente>>> GetEstoqueByCliente(int clienteId)
        {
            return await _context.EstoqueClientes
                .Include(e => e.Produto)
                .Where(e => e.IDCliente == clienteId)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<EstoqueCliente>> PostEstoque(EstoqueCliente estoque)
        {
            // Verificar se o produto existe
            var produto = await _context.Produtos.FindAsync(estoque.IDProduto);
            if (produto == null)
            {
                return BadRequest(new { message = "Produto não encontrado." });
            }

            // Verificar se o cliente existe
            var cliente = await _context.Clientes.FindAsync(estoque.IDCliente);
            if (cliente == null)
            {
                return BadRequest(new { message = "Cliente não encontrado." });
            }

            // Verificar se já existe estoque para este produto e cliente
            var estoqueExistente = await _context.EstoqueClientes
                .FirstOrDefaultAsync(e => e.IDCliente == estoque.IDCliente && e.IDProduto == estoque.IDProduto);

            if (estoqueExistente != null)
            {
                return Conflict(new { message = "Já existe estoque para este produto e cliente." });
            }

            _context.EstoqueClientes.Add(estoque);
            await _context.SaveChangesAsync();

            // Retornar o estoque com o produto carregado
            var estoqueCompleto = await _context.EstoqueClientes
                .Include(e => e.Produto)
                .FirstOrDefaultAsync(e => e.IDEstoque == estoque.IDEstoque);

            return CreatedAtAction(nameof(GetEstoqueByCliente), new { clienteId = estoque.IDCliente }, estoqueCompleto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstoque(int id, EstoqueCliente estoque)
        {
            if (id != estoque.IDEstoque)
            {
                return BadRequest();
            }

            // Verificar se o estoque existe
            var estoqueExistente = await _context.EstoqueClientes.FindAsync(id);
            if (estoqueExistente == null)
            {
                return NotFound();
            }

            // Verificar se o produto existe
            var produto = await _context.Produtos.FindAsync(estoque.IDProduto);
            if (produto == null)
            {
                return BadRequest(new { message = "Produto não encontrado." });
            }

            // Verificar se o cliente existe
            var cliente = await _context.Clientes.FindAsync(estoque.IDCliente);
            if (cliente == null)
            {
                return BadRequest(new { message = "Cliente não encontrado." });
            }

            // Verificar se já existe outro estoque para este produto e cliente (exceto o atual)
            var outroEstoque = await _context.EstoqueClientes
                .FirstOrDefaultAsync(e => e.IDCliente == estoque.IDCliente && 
                                        e.IDProduto == estoque.IDProduto && 
                                        e.IDEstoque != id);

            if (outroEstoque != null)
            {
                return Conflict(new { message = "Já existe estoque para este produto e cliente." });
            }

            _context.Entry(estoque).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstoqueExists(id))
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
        public async Task<IActionResult> DeleteEstoque(int id)
        {
            var estoque = await _context.EstoqueClientes.FindAsync(id);
            if (estoque == null)
            {
                return NotFound();
            }

            _context.EstoqueClientes.Remove(estoque);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EstoqueExists(int id)
        {
            return _context.EstoqueClientes.Any(e => e.IDEstoque == id);
        }
    }
} 