using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacoesEstoqueController : ControllerBase
    {
        private static readonly string[] TiposPermitidosNaTela = { "Entrada", "Ajuste", "Inventario" };

        private readonly GestaoPiscinaContext _context;

        public MovimentacoesEstoqueController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        // GET: api/movimentacoesestoque/cliente/{clienteId}/produto/{produtoId}
        [HttpGet("cliente/{clienteId}/produto/{produtoId}")]
        public async Task<ActionResult<IEnumerable<MovimentacaoEstoque>>> GetHistorico(int clienteId, int produtoId)
        {
            return await _context.MovimentacoesEstoque
                .AsNoTracking()
                .Where(m => m.IDCliente == clienteId && m.IDProduto == produtoId)
                .OrderByDescending(m => m.Data)
                .ThenByDescending(m => m.IDMovimentacao)
                .ToListAsync();
        }

        // POST: api/movimentacoesestoque — Entrada, Ajuste ou Inventário. Saída é
        // gerada só automaticamente pelo DosagensController ao dosar um produto numa OS.
        [HttpPost]
        public async Task<ActionResult<MovimentacaoEstoque>> PostMovimentacao(MovimentacaoEstoque movimentacao)
        {
            movimentacao.Cliente = null!;
            movimentacao.Produto = null!;
            movimentacao.Dosagem = null;
            movimentacao.IDDosagem = null;
            movimentacao.IDMovimentacao = 0;

            if (!TiposPermitidosNaTela.Contains(movimentacao.Tipo))
            {
                return BadRequest(new { message = "Tipo de movimentação inválido. Use Entrada, Ajuste ou Inventario." });
            }

            var cliente = await _context.Clientes.FindAsync(movimentacao.IDCliente);
            if (cliente == null)
            {
                return BadRequest(new { message = "Cliente não encontrado." });
            }

            var produto = await _context.Produtos.FindAsync(movimentacao.IDProduto);
            if (produto == null)
            {
                return BadRequest(new { message = "Produto não encontrado." });
            }

            var estoque = await _context.EstoqueClientes
                .FirstOrDefaultAsync(e => e.IDCliente == movimentacao.IDCliente && e.IDProduto == movimentacao.IDProduto);

            if (estoque == null)
            {
                // Uma Entrada pode "estrear" o produto no estoque do cliente; Ajuste e
                // Inventário exigem que o produto já esteja sendo acompanhado.
                if (movimentacao.Tipo != "Entrada")
                {
                    return BadRequest(new { message = "Este produto ainda não está cadastrado no estoque deste cliente. Registre uma Entrada primeiro." });
                }

                estoque = new EstoqueCliente { IDCliente = movimentacao.IDCliente, IDProduto = movimentacao.IDProduto };
                _context.EstoqueClientes.Add(estoque);
            }

            var saldoAtual = await EstoqueCalculo.SaldoAsync(_context, movimentacao.IDCliente, movimentacao.IDProduto);

            switch (movimentacao.Tipo)
            {
                case "Entrada":
                    if (movimentacao.Quantidade <= 0)
                    {
                        return BadRequest(new { message = "A quantidade de entrada deve ser maior que zero." });
                    }
                    movimentacao.QuantidadeContada = null;
                    break;

                case "Ajuste":
                    if (movimentacao.Quantidade == 0)
                    {
                        return BadRequest(new { message = "Informe uma quantidade diferente de zero para o ajuste." });
                    }
                    if (string.IsNullOrWhiteSpace(movimentacao.Observacao))
                    {
                        return BadRequest(new { message = "Informe o motivo do ajuste." });
                    }
                    movimentacao.QuantidadeContada = null;
                    break;

                case "Inventario":
                    if (movimentacao.QuantidadeContada == null || movimentacao.QuantidadeContada < 0)
                    {
                        return BadRequest(new { message = "Informe a quantidade contada no inventário." });
                    }
                    // O delta é sempre recalculado no servidor a partir do saldo mais
                    // recente, para não confiar num saldo potencialmente desatualizado
                    // que o cliente tenha usado para montar a requisição.
                    movimentacao.Quantidade = movimentacao.QuantidadeContada.Value - saldoAtual;
                    break;
            }

            var novoSaldo = saldoAtual + movimentacao.Quantidade;
            if (novoSaldo < 0)
            {
                return BadRequest(new { message = $"Essa movimentação deixaria o estoque negativo (saldo atual: {saldoAtual} {produto.Unidade})." });
            }

            movimentacao.Data = movimentacao.Data == default ? DateTime.Now : movimentacao.Data;

            _context.MovimentacoesEstoque.Add(movimentacao);
            await _context.SaveChangesAsync();

            var movimentacaoCompleta = await _context.MovimentacoesEstoque
                .AsNoTracking()
                .Include(m => m.Produto)
                .FirstOrDefaultAsync(m => m.IDMovimentacao == movimentacao.IDMovimentacao);

            return CreatedAtAction(nameof(GetHistorico), new { clienteId = movimentacao.IDCliente, produtoId = movimentacao.IDProduto }, movimentacaoCompleta);
        }

        // DELETE: api/movimentacoesestoque/{id} — só para lançamentos manuais
        // (Entrada/Ajuste/Inventario). Saídas são geridas pela OS correspondente.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovimentacao(int id)
        {
            var movimentacao = await _context.MovimentacoesEstoque.FindAsync(id);
            if (movimentacao == null)
            {
                return NotFound();
            }

            if (movimentacao.Tipo == "Saida")
            {
                return BadRequest(new { message = "Esta saída foi gerada por uma dosagem na Ordem de Serviço. Para desfazê-la, edite ou exclua a dosagem na respectiva OS." });
            }

            var saldoAtual = await EstoqueCalculo.SaldoAsync(_context, movimentacao.IDCliente, movimentacao.IDProduto);
            if (saldoAtual - movimentacao.Quantidade < 0)
            {
                return BadRequest(new { message = "Não é possível excluir esta movimentação: o estoque ficaria negativo." });
            }

            _context.MovimentacoesEstoque.Remove(movimentacao);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
