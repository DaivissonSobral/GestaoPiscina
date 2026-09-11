using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DosagensController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public DosagensController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet("os/{osId}")]
        public async Task<ActionResult<IEnumerable<DosagemProduto>>> GetDosagensPorOS(int osId)
        {
            return await _context.DosagensProdutos
                .AsNoTracking()
                .Include(d => d.Produto)
                .Where(d => d.IDOS == osId)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<DosagemProduto>> PostDosagem(DosagemProduto dosagem)
        {
            dosagem.OrdemDeServico = null!;
            dosagem.Produto = null!;

            var os = await _context.OrdensDeServico
                .Include(o => o.Piscina)
                .FirstOrDefaultAsync(o => o.IDOS == dosagem.IDOS);
            if (os == null)
            {
                return BadRequest(new { message = "Ordem de serviço não encontrada." });
            }

            var estoque = await _context.EstoqueClientes
                .Include(e => e.Produto)
                .FirstOrDefaultAsync(e => e.IDCliente == os.Piscina.IDCliente && e.IDProduto == dosagem.IDProduto);
            if (estoque == null)
            {
                return BadRequest(new { message = "Este produto não está cadastrado no estoque do cliente." });
            }

            var saldoAtual = await EstoqueCalculo.SaldoAsync(_context, os.Piscina.IDCliente, dosagem.IDProduto);
            if (saldoAtual - dosagem.Quantidade < 0)
            {
                return BadRequest(new { message = $"Estoque insuficiente para dosar {dosagem.Quantidade} {estoque.Produto.Unidade} de {estoque.Produto.Nome} (saldo atual: {saldoAtual} {estoque.Produto.Unidade})." });
            }

            _context.DosagensProdutos.Add(dosagem);

            var movimentacao = new MovimentacaoEstoque
            {
                IDCliente = os.Piscina.IDCliente,
                IDProduto = dosagem.IDProduto,
                Tipo = "Saida",
                Quantidade = -dosagem.Quantidade,
                Data = DateTime.Now,
                Observacao = $"Saída automática por dosagem na OS #{dosagem.IDOS}",
                // Referenciar via navegação (em vez do FK IDDosagem direto) permite ao EF
                // resolver a chave de dosagem só depois de inserida, no mesmo SaveChanges.
                Dosagem = dosagem
            };
            _context.MovimentacoesEstoque.Add(movimentacao);

            await _context.SaveChangesAsync();

            var dosagemCompleta = await _context.DosagensProdutos
                .AsNoTracking()
                .Include(d => d.Produto)
                .FirstOrDefaultAsync(d => d.IDDosagem == dosagem.IDDosagem);

            return CreatedAtAction(nameof(GetDosagensPorOS), new { osId = dosagem.IDOS }, dosagemCompleta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDosagem(int id, DosagemProduto dosagem)
        {
            if (id != dosagem.IDDosagem)
            {
                return BadRequest();
            }

            var dosagemExistente = await _context.DosagensProdutos
                .Include(d => d.OrdemDeServico)
                .ThenInclude(o => o.Piscina)
                .FirstOrDefaultAsync(d => d.IDDosagem == id);
            if (dosagemExistente == null)
            {
                return NotFound();
            }

            var idCliente = dosagemExistente.OrdemDeServico.Piscina.IDCliente;

            var estoque = await _context.EstoqueClientes
                .Include(e => e.Produto)
                .FirstOrDefaultAsync(e => e.IDCliente == idCliente && e.IDProduto == dosagemExistente.IDProduto);
            if (estoque == null)
            {
                return BadRequest(new { message = "Este produto não está mais cadastrado no estoque do cliente." });
            }

            // O incremento líquido a aplicar ao saldo é sempre "quantidade antiga menos
            // quantidade nova" (dosar mais consome mais do saldo), independente de já
            // existir ou não uma movimentação de Saída vinculada — ver comentário abaixo.
            var quantidadeAntiga = dosagemExistente.Quantidade;
            var saldoAtual = await EstoqueCalculo.SaldoAsync(_context, idCliente, dosagemExistente.IDProduto);
            var novoSaldo = saldoAtual + quantidadeAntiga - dosagem.Quantidade;
            if (novoSaldo < 0)
            {
                return BadRequest(new { message = $"Estoque insuficiente para essa quantidade (saldo atual: {saldoAtual} {estoque.Produto.Unidade})." });
            }

            var movimentacaoVinculada = await _context.MovimentacoesEstoque
                .FirstOrDefaultAsync(m => m.IDDosagem == id);

            if (movimentacaoVinculada != null)
            {
                // A movimentação vinculada já representa integralmente o efeito desta
                // dosagem no saldo (seja porque foi criada assim no Post, seja porque já
                // passou pelo "curativo" abaixo antes) — pode ser sobrescrita direto.
                movimentacaoVinculada.Quantidade = -dosagem.Quantidade;
            }
            else
            {
                // Primeira edição de uma dosagem criada antes do livro-razão de estoque:
                // seu efeito original está embutido de forma implícita no saldo de
                // abertura (sem nenhuma movimentação própria). Para não duplicar nem
                // perder esse efeito, extrai a quantidade original do saldo de abertura
                // com um Ajuste equivalente e, só então, registra uma Saída vinculada que
                // passa a representar o efeito completo da dosagem — assim, futuras
                // edições caem sempre no ramo acima (a dosagem fica "curada" para sempre).
                _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                {
                    IDCliente = idCliente,
                    IDProduto = dosagemExistente.IDProduto,
                    Tipo = "Ajuste",
                    Quantidade = quantidadeAntiga,
                    Data = DateTime.Now,
                    Observacao = $"Extração do consumo original (dosagem migrada) da OS #{dosagemExistente.IDOS}"
                });
                _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                {
                    IDCliente = idCliente,
                    IDProduto = dosagemExistente.IDProduto,
                    Tipo = "Saida",
                    Quantidade = -dosagem.Quantidade,
                    Data = DateTime.Now,
                    Observacao = $"Saída por dosagem na OS #{dosagemExistente.IDOS}",
                    IDDosagem = id
                });
            }

            dosagemExistente.Quantidade = dosagem.Quantidade;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDosagem(int id)
        {
            var dosagem = await _context.DosagensProdutos
                .Include(d => d.OrdemDeServico)
                .ThenInclude(o => o.Piscina)
                .FirstOrDefaultAsync(d => d.IDDosagem == id);
            if (dosagem == null)
            {
                return NotFound();
            }

            var movimentacaoVinculada = await _context.MovimentacoesEstoque
                .FirstOrDefaultAsync(m => m.IDDosagem == id);

            if (movimentacaoVinculada != null)
            {
                _context.MovimentacoesEstoque.Remove(movimentacaoVinculada);
            }
            else
            {
                // Idem PutDosagem: sem movimentação vinculada, o consumo original está
                // embutido no saldo de abertura — para devolver a quantidade, registra
                // um ajuste de estorno em vez de simplesmente não fazer nada.
                _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                {
                    IDCliente = dosagem.OrdemDeServico.Piscina.IDCliente,
                    IDProduto = dosagem.IDProduto,
                    Tipo = "Ajuste",
                    Quantidade = dosagem.Quantidade,
                    Data = DateTime.Now,
                    Observacao = $"Estorno de dosagem excluída (migrada) na OS #{dosagem.IDOS}"
                });
            }

            _context.DosagensProdutos.Remove(dosagem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
