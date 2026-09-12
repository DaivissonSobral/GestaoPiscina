using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Controllers;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Controllers
{
    // RN03/RF05 – a dosagem de produtos numa OS deve refletir corretamente no
    // livro-razão de estoque do cliente (ver comentários em DosagensController).
    public class DosagensControllerTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();
        private DosagensController Controller => new(_db.Context);

        private async Task<(OrdemDeServico os, EstoqueCliente estoque)> CriarCenarioComEstoqueAsync(decimal saldoInicial)
        {
            var perfil = Fabrica.Perfil("Técnico");
            var tecnico = Fabrica.Usuario(perfil);
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente);
            var produto = Fabrica.Produto();
            _db.Context.AddRange(perfil, tecnico, cliente, piscina, produto);
            await _db.Context.SaveChangesAsync();

            var estoque = Fabrica.Estoque(cliente, produto);
            _db.Context.EstoqueClientes.Add(estoque);
            if (saldoInicial != 0)
            {
                _db.Context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                {
                    IDCliente = cliente.IDCliente,
                    IDProduto = produto.IDProduto,
                    Tipo = "Entrada",
                    Quantidade = saldoInicial,
                    Data = DateTime.Now
                });
            }

            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            _db.Context.OrdensDeServico.Add(os);
            await _db.Context.SaveChangesAsync();

            return (os, estoque);
        }

        [Fact]
        public async Task PostDosagem_ComEstoqueSuficiente_CriaDosagemEDebitaEstoque()
        {
            var (os, estoque) = await CriarCenarioComEstoqueAsync(saldoInicial: 10m);

            var resultado = await Controller.PostDosagem(new DosagemProduto
            {
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 3m
            });

            Assert.IsType<CreatedAtActionResult>(resultado.Result);
            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, estoque.IDCliente, estoque.IDProduto);
            Assert.Equal(7m, saldo);
        }

        [Fact]
        public async Task PostDosagem_ComEstoqueInsuficiente_RetornaBadRequestSemAlterarEstoque()
        {
            var (os, estoque) = await CriarCenarioComEstoqueAsync(saldoInicial: 2m);

            var resultado = await Controller.PostDosagem(new DosagemProduto
            {
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 5m
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, estoque.IDCliente, estoque.IDProduto);
            Assert.Equal(2m, saldo);
        }

        [Fact]
        public async Task PostDosagem_ComOSInexistente_RetornaBadRequest()
        {
            var resultado = await Controller.PostDosagem(new DosagemProduto
            {
                IDOS = 999999,
                IDProduto = 1,
                Quantidade = 1m
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostDosagem_ComProdutoNaoAcompanhadoNoEstoqueDoCliente_RetornaBadRequest()
        {
            var perfil = Fabrica.Perfil("Técnico");
            var tecnico = Fabrica.Usuario(perfil);
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente);
            var produtoForaDoEstoque = Fabrica.Produto("Produto Não Cadastrado");
            _db.Context.AddRange(perfil, tecnico, cliente, piscina, produtoForaDoEstoque);
            await _db.Context.SaveChangesAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            _db.Context.OrdensDeServico.Add(os);
            await _db.Context.SaveChangesAsync();

            var resultado = await Controller.PostDosagem(new DosagemProduto
            {
                IDOS = os.IDOS,
                IDProduto = produtoForaDoEstoque.IDProduto,
                Quantidade = 1m
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PutDosagem_AumentandoQuantidade_DebitaADiferencaDoEstoque()
        {
            var (os, estoque) = await CriarCenarioComEstoqueAsync(saldoInicial: 10m);
            var criada = (DosagemProduto)((CreatedAtActionResult)(await Controller.PostDosagem(new DosagemProduto
            {
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 3m
            })).Result!).Value!;
            // Saldo agora: 10 - 3 = 7.

            var resultado = await Controller.PutDosagem(criada.IDDosagem, new DosagemProduto
            {
                IDDosagem = criada.IDDosagem,
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 5m
            });

            Assert.IsType<NoContentResult>(resultado);
            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, estoque.IDCliente, estoque.IDProduto);
            Assert.Equal(5m, saldo); // 10 - 5
        }

        [Fact]
        public async Task PutDosagem_DiminuindoQuantidade_CreditaADiferencaAoEstoque()
        {
            var (os, estoque) = await CriarCenarioComEstoqueAsync(saldoInicial: 10m);
            var criada = (DosagemProduto)((CreatedAtActionResult)(await Controller.PostDosagem(new DosagemProduto
            {
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 5m
            })).Result!).Value!;
            // Saldo agora: 10 - 5 = 5.

            var resultado = await Controller.PutDosagem(criada.IDDosagem, new DosagemProduto
            {
                IDDosagem = criada.IDDosagem,
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 2m
            });

            Assert.IsType<NoContentResult>(resultado);
            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, estoque.IDCliente, estoque.IDProduto);
            Assert.Equal(8m, saldo); // 10 - 2
        }

        [Fact]
        public async Task PutDosagem_ComEstoqueInsuficienteParaOAumento_RetornaBadRequestSemAlterarEstoque()
        {
            var (os, estoque) = await CriarCenarioComEstoqueAsync(saldoInicial: 10m);
            var criada = (DosagemProduto)((CreatedAtActionResult)(await Controller.PostDosagem(new DosagemProduto
            {
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 3m
            })).Result!).Value!;
            // Saldo agora: 7.

            var resultado = await Controller.PutDosagem(criada.IDDosagem, new DosagemProduto
            {
                IDDosagem = criada.IDDosagem,
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 999m
            });

            Assert.IsType<BadRequestObjectResult>(resultado);
            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, estoque.IDCliente, estoque.IDProduto);
            Assert.Equal(7m, saldo);
        }

        [Fact]
        public async Task PutDosagem_SemMovimentacaoVinculada_ExtraiOConsumoOriginalEDeixaADosagemCurada()
        {
            // Simula uma dosagem criada antes do livro-razão existir: registro em
            // DosagensProdutos sem nenhuma MovimentacaoEstoque vinculada (IDDosagem).
            var (os, estoque) = await CriarCenarioComEstoqueAsync(saldoInicial: 20m);
            var dosagemMigrada = new DosagemProduto { IDOS = os.IDOS, IDProduto = estoque.IDProduto, Quantidade = 4m };
            _db.Context.DosagensProdutos.Add(dosagemMigrada);
            await _db.Context.SaveChangesAsync();
            // Saldo de abertura já embutia o consumo dos 4 — nenhuma movimentação criada ainda.
            Assert.Equal(20m, await EstoqueCalculo.SaldoAsync(_db.Context, estoque.IDCliente, estoque.IDProduto));

            var resultado = await Controller.PutDosagem(dosagemMigrada.IDDosagem, new DosagemProduto
            {
                IDDosagem = dosagemMigrada.IDDosagem,
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 6m
            });

            Assert.IsType<NoContentResult>(resultado);
            // O saldo de abertura (20) é tratado como se já refletisse o consumo histórico
            // dos 4 originais — por isso o Ajuste de +4 "desfaz" esse desconto implícito
            // antes de aplicar a Saída de -6 da nova quantidade: 20 + 4 - 6 = 18, ou seja,
            // o efeito líquido é só a diferença (6 - 4 = 2) a mais de consumo.
            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, estoque.IDCliente, estoque.IDProduto);
            Assert.Equal(18m, saldo);
            Assert.NotNull(await _db.Context.MovimentacoesEstoque.FirstOrDefaultAsync(m => m.IDDosagem == dosagemMigrada.IDDosagem));
        }

        [Fact]
        public async Task DeleteDosagem_ComMovimentacaoVinculada_RemoveAMovimentacaoERestauraOEstoque()
        {
            var (os, estoque) = await CriarCenarioComEstoqueAsync(saldoInicial: 10m);
            var criada = (DosagemProduto)((CreatedAtActionResult)(await Controller.PostDosagem(new DosagemProduto
            {
                IDOS = os.IDOS,
                IDProduto = estoque.IDProduto,
                Quantidade = 4m
            })).Result!).Value!;
            // Saldo agora: 6.

            var resultado = await Controller.DeleteDosagem(criada.IDDosagem);

            Assert.IsType<NoContentResult>(resultado);
            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, estoque.IDCliente, estoque.IDProduto);
            Assert.Equal(10m, saldo);
            Assert.Null(await _db.Context.DosagensProdutos.FindAsync(criada.IDDosagem));
        }

        [Fact]
        public async Task DeleteDosagem_SemMovimentacaoVinculada_CriaAjusteDeEstornoRestaurandoOEstoque()
        {
            var (os, estoque) = await CriarCenarioComEstoqueAsync(saldoInicial: 20m);
            var dosagemMigrada = new DosagemProduto { IDOS = os.IDOS, IDProduto = estoque.IDProduto, Quantidade = 4m };
            _db.Context.DosagensProdutos.Add(dosagemMigrada);
            await _db.Context.SaveChangesAsync();

            var resultado = await Controller.DeleteDosagem(dosagemMigrada.IDDosagem);

            Assert.IsType<NoContentResult>(resultado);
            // O saldo de abertura já embutia o consumo — o estorno deve aumentar o saldo em +4.
            var saldo = await EstoqueCalculo.SaldoAsync(_db.Context, estoque.IDCliente, estoque.IDProduto);
            Assert.Equal(24m, saldo);
        }

        [Fact]
        public async Task DeleteDosagem_Inexistente_RetornaNotFound()
        {
            var resultado = await Controller.DeleteDosagem(999999);

            Assert.IsType<NotFoundResult>(resultado);
        }

        public void Dispose() => _db.Dispose();
    }
}
