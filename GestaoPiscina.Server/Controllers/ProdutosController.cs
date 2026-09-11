using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public ProdutosController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos
                .Include(p => p.Estoques)
                .ToListAsync();
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutosByCliente(int clienteId)
        {
            // Verificar se o cliente existe
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                return NotFound($"Cliente com ID {clienteId} não encontrado");
            }
            
            // Buscar produtos que estão em estoque do cliente específico (saldo > 0,
            // calculado a partir do livro-razão de movimentações)
            var estoquesDoCliente = await _context.EstoqueClientes
                .Where(ec => ec.IDCliente == clienteId)
                .Include(ec => ec.Produto)
                .ToListAsync();

            var saldos = await EstoqueCalculo.SaldosAsync(_context);

            var produtos = estoquesDoCliente
                .Where(ec => saldos.GetValueOrDefault((ec.IDCliente, ec.IDProduto)) > 0)
                .Select(ec => ec.Produto)
                .Distinct()
                .ToList();

            return produtos;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Estoques)
                .FirstOrDefaultAsync(p => p.IDProduto == id);

            if (produto == null)
            {
                return NotFound();
            }

            return produto;
        }

        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(Produto produto)
        {
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduto), new { id = produto.IDProduto }, produto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, Produto produto)
        {
            if (id != produto.IDProduto)
            {
                return BadRequest();
            }

            _context.Entry(produto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProdutoExists(id))
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
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.IDProduto == id);
        }
    }
} 