using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;

namespace GestaoPiscina.Server.Services
{
    // Calcula o saldo atual de estoque (Cliente, Produto) a partir da soma dos
    // lançamentos em MovimentacaoEstoque — o saldo nunca é lido de um campo
    // armazenado diretamente (ver comentário em EstoqueCliente.QuantidadeAtual).
    public static class EstoqueCalculo
    {
        public static async Task<decimal> SaldoAsync(GestaoPiscinaContext context, int idCliente, int idProduto)
        {
            return await context.MovimentacoesEstoque
                .Where(m => m.IDCliente == idCliente && m.IDProduto == idProduto)
                .SumAsync(m => (decimal?)m.Quantidade) ?? 0m;
        }

        // Traz o saldo de todos os pares (Cliente, Produto) que têm ao menos um
        // lançamento, num só round-trip — usado pelas listagens (evita N+1).
        public static async Task<Dictionary<(int IDCliente, int IDProduto), decimal>> SaldosAsync(GestaoPiscinaContext context)
        {
            var grupos = await context.MovimentacoesEstoque
                .GroupBy(m => new { m.IDCliente, m.IDProduto })
                .Select(g => new { g.Key.IDCliente, g.Key.IDProduto, Saldo = g.Sum(m => m.Quantidade) })
                .ToListAsync();

            return grupos.ToDictionary(g => (g.IDCliente, g.IDProduto), g => g.Saldo);
        }

        // Data do lançamento mais recente de cada par (Cliente, Produto) — usada para
        // o filtro "última movimentação" da tela de Estoque.
        public static async Task<Dictionary<(int IDCliente, int IDProduto), DateTime>> UltimasMovimentacoesAsync(GestaoPiscinaContext context)
        {
            var grupos = await context.MovimentacoesEstoque
                .GroupBy(m => new { m.IDCliente, m.IDProduto })
                .Select(g => new { g.Key.IDCliente, g.Key.IDProduto, Data = g.Max(m => m.Data) })
                .ToListAsync();

            return grupos.ToDictionary(g => (g.IDCliente, g.IDProduto), g => g.Data);
        }
    }
}
