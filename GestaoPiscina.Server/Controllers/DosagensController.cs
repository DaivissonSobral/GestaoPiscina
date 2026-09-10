using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

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
                .FirstOrDefaultAsync(e => e.IDCliente == os.Piscina.IDCliente && e.IDProduto == dosagem.IDProduto);
            if (estoque == null)
            {
                return BadRequest(new { message = "Este produto não está cadastrado no estoque do cliente." });
            }

            estoque.QuantidadeAtual -= dosagem.Quantidade;

            _context.DosagensProdutos.Add(dosagem);
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

            var estoque = await _context.EstoqueClientes
                .FirstOrDefaultAsync(e => e.IDCliente == dosagemExistente.OrdemDeServico.Piscina.IDCliente && e.IDProduto == dosagemExistente.IDProduto);
            if (estoque == null)
            {
                return BadRequest(new { message = "Este produto não está mais cadastrado no estoque do cliente." });
            }

            // Devolve a quantidade antiga ao estoque e aplica a nova.
            estoque.QuantidadeAtual += dosagemExistente.Quantidade;
            estoque.QuantidadeAtual -= dosagem.Quantidade;
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

            var estoque = await _context.EstoqueClientes
                .FirstOrDefaultAsync(e => e.IDCliente == dosagem.OrdemDeServico.Piscina.IDCliente && e.IDProduto == dosagem.IDProduto);
            if (estoque != null)
            {
                estoque.QuantidadeAtual += dosagem.Quantidade;
            }

            _context.DosagensProdutos.Remove(dosagem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
