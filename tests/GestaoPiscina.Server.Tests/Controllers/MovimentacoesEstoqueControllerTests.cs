using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Controllers;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Controllers
{
    // RN07 – Controle de Estoque do Cliente: lançamentos manuais (Entrada,
    // Ajuste, Inventário) feitos na tela de Estoque.
    public class MovimentacoesEstoqueControllerTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();
        private MovimentacoesEstoqueController Controller => new(_db.Context);

        private async Task<(Cliente cliente, Produto produto)> CriarClienteEProdutoAsync()
        {
            var cliente = Fabrica.Cliente();
            var produto = Fabrica.Produto();
            _db.Context.AddRange(cliente, produto);
            await _db.Context.SaveChangesAsync();
            return (cliente, produto);
        }

        [Fact]
        public async Task PostMovimentacao_Entrada_QuandoProdutoAindaNaoEstaNoEstoque_CriaOVinculoEOSaldo()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();

            var resultado = await Controller.PostMovimentacao(new MovimentacaoEstoque
            {
                IDCliente = cliente.IDCliente,
                IDProduto = produto.IDProduto,
                Tipo = "Entrada",
                Quantidade = 25m
            });

            Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(25m, await EstoqueCalculo.SaldoAsync(_db.Context, cliente.IDCliente, produto.IDProduto));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task PostMovimentacao_Entrada_ComQuantidadeNaoPositiva_RetornaBadRequest(decimal quantidade)
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();

            var resultado = await Controller.PostMovimentacao(new MovimentacaoEstoque
            {
                IDCliente = cliente.IDCliente,
                IDProduto = produto.IDProduto,
                Tipo = "Entrada",
                Quantidade = quantidade
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostMovimentacao_Ajuste_QuandoProdutoAindaNaoEstaNoEstoque_RetornaBadRequest()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();

            var resultado = await Controller.PostMovimentacao(new MovimentacaoEstoque
            {
                IDCliente = cliente.IDCliente,
                IDProduto = produto.IDProduto,
                Tipo = "Ajuste",
                Quantidade = 5m,
                Observacao = "Correção"
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostMovimentacao_Ajuste_SemObservacao_RetornaBadRequest()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();
            await Controller.PostMovimentacao(new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 10m });

            var resultado = await Controller.PostMovimentacao(new MovimentacaoEstoque
            {
                IDCliente = cliente.IDCliente,
                IDProduto = produto.IDProduto,
                Tipo = "Ajuste",
                Quantidade = -2m,
                Observacao = null
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostMovimentacao_Ajuste_ComQuantidadeZero_RetornaBadRequest()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();
            await Controller.PostMovimentacao(new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 10m });

            var resultado = await Controller.PostMovimentacao(new MovimentacaoEstoque
            {
                IDCliente = cliente.IDCliente,
                IDProduto = produto.IDProduto,
                Tipo = "Ajuste",
                Quantidade = 0m,
                Observacao = "Sem efeito"
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostMovimentacao_Inventario_RecalculaADiferencaAPartirDoSaldoAtualNoServidor()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();
            await Controller.PostMovimentacao(new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 10m });
            // Saldo atual: 10. O cliente informa que a contagem física deu 8 — mesmo
            // que ele tenha enviado uma "Quantidade" diferente, o delta real (-2) é
            // quem deve prevalecer, recalculado no servidor.

            var resultado = await Controller.PostMovimentacao(new MovimentacaoEstoque
            {
                IDCliente = cliente.IDCliente,
                IDProduto = produto.IDProduto,
                Tipo = "Inventario",
                QuantidadeContada = 8m,
                Quantidade = 999m // deve ser ignorado/recalculado pelo servidor
            });

            Assert.IsType<CreatedAtActionResult>(resultado.Result);
            Assert.Equal(8m, await EstoqueCalculo.SaldoAsync(_db.Context, cliente.IDCliente, produto.IDProduto));
        }

        [Fact]
        public async Task PostMovimentacao_Inventario_SemQuantidadeContada_RetornaBadRequest()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();
            await Controller.PostMovimentacao(new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 10m });

            var resultado = await Controller.PostMovimentacao(new MovimentacaoEstoque
            {
                IDCliente = cliente.IDCliente,
                IDProduto = produto.IDProduto,
                Tipo = "Inventario",
                QuantidadeContada = null
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostMovimentacao_QueDeixariaOEstoqueNegativo_RetornaBadRequestSemAlterarOSaldo()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();
            await Controller.PostMovimentacao(new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 5m });

            var resultado = await Controller.PostMovimentacao(new MovimentacaoEstoque
            {
                IDCliente = cliente.IDCliente,
                IDProduto = produto.IDProduto,
                Tipo = "Ajuste",
                Quantidade = -10m,
                Observacao = "Correção grande demais"
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
            Assert.Equal(5m, await EstoqueCalculo.SaldoAsync(_db.Context, cliente.IDCliente, produto.IDProduto));
        }

        [Fact]
        public async Task PostMovimentacao_TipoNaoPermitidoNestaTela_RetornaBadRequest()
        {
            // "Saida" só é gerada automaticamente por dosagem numa OS (DosagensController),
            // nunca lançada manualmente por este endpoint.
            var (cliente, produto) = await CriarClienteEProdutoAsync();

            var resultado = await Controller.PostMovimentacao(new MovimentacaoEstoque
            {
                IDCliente = cliente.IDCliente,
                IDProduto = produto.IDProduto,
                Tipo = "Saida",
                Quantidade = -1m
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task DeleteMovimentacao_DoTipoSaida_RetornaBadRequest()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();
            var saida = new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Saida", Quantidade = -1m, Data = DateTime.Now };
            _db.Context.MovimentacoesEstoque.Add(saida);
            await _db.Context.SaveChangesAsync();

            var resultado = await Controller.DeleteMovimentacao(saida.IDMovimentacao);

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task DeleteMovimentacao_QueDeixariaOEstoqueNegativo_RetornaBadRequest()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();
            await Controller.PostMovimentacao(new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 5m });
            var saida = new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Saida", Quantidade = -3m, Data = DateTime.Now };
            _db.Context.MovimentacoesEstoque.Add(saida);
            await _db.Context.SaveChangesAsync();
            // Saldo atual: 2. Excluir a Entrada de 5 deixaria o saldo em -3.
            var entrada = await _db.Context.MovimentacoesEstoque.FirstAsync(m => m.Tipo == "Entrada");

            var resultado = await Controller.DeleteMovimentacao(entrada.IDMovimentacao);

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task DeleteMovimentacao_ManualValida_RemoveComSucesso()
        {
            var (cliente, produto) = await CriarClienteEProdutoAsync();
            await Controller.PostMovimentacao(new MovimentacaoEstoque { IDCliente = cliente.IDCliente, IDProduto = produto.IDProduto, Tipo = "Entrada", Quantidade = 10m });
            var entrada = await _db.Context.MovimentacoesEstoque.FirstAsync();

            var resultado = await Controller.DeleteMovimentacao(entrada.IDMovimentacao);

            Assert.IsType<NoContentResult>(resultado);
            Assert.Equal(0m, await EstoqueCalculo.SaldoAsync(_db.Context, cliente.IDCliente, produto.IDProduto));
        }

        [Fact]
        public async Task DeleteMovimentacao_Inexistente_RetornaNotFound()
        {
            var resultado = await Controller.DeleteMovimentacao(999999);

            Assert.IsType<NotFoundResult>(resultado);
        }

        public void Dispose() => _db.Dispose();
    }
}
