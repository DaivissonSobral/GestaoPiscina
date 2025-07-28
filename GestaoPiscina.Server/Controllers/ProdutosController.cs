using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

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
            
            // Buscar produtos que estão em estoque do cliente específico
            var produtos = await _context.EstoquesCliente
                .Where(ec => ec.IDCliente == clienteId && ec.QuantidadeAtual > 0)
                .Include(ec => ec.Produto)
                .Select(ec => ec.Produto)
                .Distinct()
                .ToListAsync();

            return produtos;
        }

        [HttpGet("debug/cliente/{clienteId}")]
        public async Task<ActionResult<object>> DebugProdutosByCliente(int clienteId)
        {
            // Verificar se o cliente existe
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                return NotFound($"Cliente com ID {clienteId} não encontrado");
            }

            // Verificar estoques do cliente
            var estoques = await _context.EstoquesCliente
                .Where(ec => ec.IDCliente == clienteId)
                .Include(ec => ec.Produto)
                .ToListAsync();

            // Verificar produtos com estoque > 0
            var produtosComEstoque = await _context.EstoquesCliente
                .Where(ec => ec.IDCliente == clienteId && ec.QuantidadeAtual > 0)
                .Include(ec => ec.Produto)
                .Select(ec => ec.Produto)
                .Distinct()
                .ToListAsync();

            // Verificar todos os produtos
            var todosProdutos = await _context.Produtos.ToListAsync();

            return new
            {
                Cliente = new { cliente.IDCliente, cliente.Nome },
                TotalEstoques = estoques.Count,
                Estoques = estoques.Select(e => new { e.IDEstoque, e.IDProduto, e.QuantidadeAtual, e.QuantidadeMinima, ProdutoNome = e.Produto?.Nome }),
                ProdutosComEstoque = produtosComEstoque.Select(p => new { p.IDProduto, p.Nome }),
                TotalProdutos = todosProdutos.Count,
                TodosProdutos = todosProdutos.Select(p => new { p.IDProduto, p.Nome })
            };
        }

        [HttpPost("estoque")]
        public async Task<ActionResult<EstoqueCliente>> AddEstoque(EstoqueCliente estoque)
        {
            // Verificar se o cliente existe
            var cliente = await _context.Clientes.FindAsync(estoque.IDCliente);
            if (cliente == null)
            {
                return NotFound($"Cliente com ID {estoque.IDCliente} não encontrado");
            }

            // Verificar se o produto existe
            var produto = await _context.Produtos.FindAsync(estoque.IDProduto);
            if (produto == null)
            {
                return NotFound($"Produto com ID {estoque.IDProduto} não encontrado");
            }

            // Verificar se já existe estoque para este cliente/produto
            var estoqueExistente = await _context.EstoquesCliente
                .FirstOrDefaultAsync(ec => ec.IDCliente == estoque.IDCliente && ec.IDProduto == estoque.IDProduto);

            if (estoqueExistente != null)
            {
                // Atualizar estoque existente
                estoqueExistente.QuantidadeAtual = estoque.QuantidadeAtual;
                estoqueExistente.QuantidadeMinima = estoque.QuantidadeMinima;
            }
            else
            {
                // Adicionar novo estoque
                _context.EstoquesCliente.Add(estoque);
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProdutosByCliente), new { clienteId = estoque.IDCliente }, estoque);
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