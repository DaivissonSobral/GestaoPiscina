using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Services
{
    // RN07 – Controle de Estoque do Cliente: o saldo é sempre derivado do
    // livro-razão (MovimentacaoEstoque), nunca de um campo armazenado.
    public class EstoqueCalculoTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();

        [Fact]
        public async Task SaldoAsync_SemMovimentacoes_RetornaZero()
        {
            var cliente = Fabrica.Cliente();
            var produto = Fabrica.Produto();
            _db.Context.AddRange(cliente, produto);
            await _db.Context.SaveChangesAsync();

            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, cliente.IDCliente, produto.IDProduto);

            Assert.Equal(0m, saldo);
        }

        [Fact]
        public async Task SaldoAsync_SomaTodosOsLancamentosDoPar()
        {
            var cliente = Fabrica.Cliente();
            var produto = Fabrica.Produto();
            _db.Context.AddRange(cliente, produto);
            await _db.Context.SaveChangesAsync();

            _db.Context.MovimentacoesEstoque.AddRange(
                new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 20m, Data = DateTime.Now },
                new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Saida", Quantidade = -5m, Data = DateTime.Now },
                new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Ajuste", Quantidade = -2m, Data = DateTime.Now });
            await _db.Context.SaveChangesAsync();

            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, cliente.IDCliente, produto.IDProduto);

            Assert.Equal(13m, saldo);
        }

        [Fact]
        public async Task SaldoAsync_NaoMisturaClientesOuProdutosDiferentes()
        {
            var clienteA = Fabrica.Cliente("Cliente A");
            var clienteB = Fabrica.Cliente("Cliente B");
            var produtoX = Fabrica.Produto("Produto X");
            var produtoY = Fabrica.Produto("Produto Y");
            _db.Context.AddRange(clienteA, clienteB, produtoX, produtoY);
            await _db.Context.SaveChangesAsync();

            _db.Context.MovimentacoesEstoque.AddRange(
                new MovimentacaoEstoque { IDCliente = clienteA.IDCliente, IDProduto = produtoX.IDProduto, Tipo = "Entrada", Quantidade = 100m, Data = DateTime.Now },
                new MovimentacaoEstoque { IDCliente = clienteB.IDCliente, IDProduto = produtoX.IDProduto, Tipo = "Entrada", Quantidade = 50m, Data = DateTime.Now },
                new MovimentacaoEstoque { IDCliente = clienteA.IDCliente, IDProduto = produtoY.IDProduto, Tipo = "Entrada", Quantidade = 30m, Data = DateTime.Now });
            await _db.Context.SaveChangesAsync();

            Assert.Equal(100m, await EstoqueCalculo.SaldoAsync(_db.Context, clienteA.IDCliente, produtoX.IDProduto));
            Assert.Equal(50m, await EstoqueCalculo.SaldoAsync(_db.Context, clienteB.IDCliente, produtoX.IDProduto));
            Assert.Equal(30m, await EstoqueCalculo.SaldoAsync(_db.Context, clienteA.IDCliente, produtoY.IDProduto));
        }

        [Fact]
        public async Task SaldosAsync_RetornaUmaEntradaPorParClienteProduto()
        {
            var cliente = Fabrica.Cliente();
            var produtoX = Fabrica.Produto("Produto X");
            var produtoY = Fabrica.Produto("Produto Y");
            _db.Context.AddRange(cliente, produtoX, produtoY);
            await _db.Context.SaveChangesAsync();

            _db.Context.MovimentacoesEstoque.AddRange(
                new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produtoX.IDProduto, Tipo = "Entrada", Quantidade = 10m, Data = DateTime.Now },
                new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produtoY.IDProduto, Tipo = "Entrada", Quantidade = 5m, Data = DateTime.Now });
            await _db.Context.SaveChangesAsync();

            var saldos = await EstoqueCalculo.SaldosAsync(_db.Context);

            Assert.Equal(10m, saldos[(cliente.IDCliente, produtoX.IDProduto)]);
            Assert.Equal(5m, saldos[(cliente.IDCliente, produtoY.IDProduto)]);
        }

        [Fact]
        public async Task UltimasMovimentacoesAsync_RetornaADataMaisRecentePorPar()
        {
            var cliente = Fabrica.Cliente();
            var produto = Fabrica.Produto();
            _db.Context.AddRange(cliente, produto);
            await _db.Context.SaveChangesAsync();

            var maisAntiga = new DateTime(2024, 1, 1);
            var maisRecente = new DateTime(2024, 6, 1);
            _db.Context.MovimentacoesEstoque.AddRange(
                new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 10m, Data = maisAntiga },
                new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 5m, Data = maisRecente });
            await _db.Context.SaveChangesAsync();

            var ultimas = await EstoqueCalculo.UltimasMovimentacoesAsync(_db.Context);

            Assert.Equal(maisRecente, ultimas[(cliente.IDCliente, produto.IDProduto)]);
        }

        public void Dispose() => _db.Dispose();
    }
}
