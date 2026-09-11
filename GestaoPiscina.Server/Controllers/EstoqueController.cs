using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstoqueCliente>>> GetEstoque()
        {
            var estoques = await _context.EstoqueClientes
                .AsNoTracking()
                .Include(e => e.Produto)
                .Include(e => e.Cliente)
                .ToListAsync();

            // Como Produto é catálogo compartilhado entre clientes, o EF acaba
            // preenchendo Cliente.Estoques/Produto.Estoques de volta para o
            // próprio registro — cria um grafo circular que estoura o limite de
            // profundidade do serializador JSON. Essas coleções reversas não são
            // usadas por quem consome este endpoint, então zeram aqui.
            var saldos = await EstoqueCalculo.SaldosAsync(_context);
            var ultimasMovimentacoes = await EstoqueCalculo.UltimasMovimentacoesAsync(_context);
            foreach (var estoque in estoques)
            {
                estoque.Cliente.Estoques.Clear();
                estoque.Produto.Estoques.Clear();
                estoque.QuantidadeAtual = saldos.GetValueOrDefault((estoque.IDCliente, estoque.IDProduto));
                if (ultimasMovimentacoes.TryGetValue((estoque.IDCliente, estoque.IDProduto), out var ultima))
                {
                    estoque.UltimaMovimentacao = ultima;
                }
            }

            return estoques;
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<EstoqueCliente>>> GetEstoqueByCliente(int clienteId)
        {
            var estoques = await _context.EstoqueClientes
                .Include(e => e.Produto)
                .Where(e => e.IDCliente == clienteId)
                .ToListAsync();

            var saldos = await EstoqueCalculo.SaldosAsync(_context);
            var ultimasMovimentacoes = await EstoqueCalculo.UltimasMovimentacoesAsync(_context);
            foreach (var estoque in estoques)
            {
                estoque.QuantidadeAtual = saldos.GetValueOrDefault((estoque.IDCliente, estoque.IDProduto));
                if (ultimasMovimentacoes.TryGetValue((estoque.IDCliente, estoque.IDProduto), out var ultima))
                {
                    estoque.UltimaMovimentacao = ultima;
                }
            }

            return estoques;
        }

        [HttpPost]
        public async Task<ActionResult<EstoqueCliente>> PostEstoque(EstoqueCliente estoque)
        {
            // O cliente pode enviar Cliente/Produto aninhados (populados só para exibição no formulário);
            // nunca confiar neles para o EF anexar/rastrear - só a chave estrangeira importa aqui.
            estoque.Cliente = null!;
            estoque.Produto = null!;

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

            // A quantidade atual não é mais um campo de entrada: o saldo começa em
            // zero e só muda através de lançamentos (Entrada/Ajuste/Inventário na
            // tela de Estoque, ou Saída automática ao dosar um produto numa OS).
            _context.EstoqueClientes.Add(estoque);
            await _context.SaveChangesAsync();

            var estoqueCompleto = await _context.EstoqueClientes
                .Include(e => e.Produto)
                .FirstOrDefaultAsync(e => e.IDEstoque == estoque.IDEstoque);

            if (estoqueCompleto != null)
            {
                estoqueCompleto.QuantidadeAtual = 0;
            }

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

            // Edição aqui só se aplica ao limite mínimo — produto e saldo não são
            // mais editáveis diretamente (saldo vem de MovimentacaoEstoque; produto
            // fica travado após o cadastro, mesma convenção usada em Piscina/Equipamento).
            estoqueExistente.QuantidadeMinima = estoque.QuantidadeMinima;

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
