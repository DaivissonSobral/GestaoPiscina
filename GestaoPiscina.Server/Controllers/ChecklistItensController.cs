using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChecklistItensController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public ChecklistItensController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChecklistItem>>> GetChecklistItens()
        {
            return await _context.ChecklistItens
                .OrderBy(c => c.Ordem)
                .ThenBy(c => c.IDChecklistItem)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChecklistItem>> GetChecklistItem(int id)
        {
            var item = await _context.ChecklistItens.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        [HttpPost]
        public async Task<ActionResult<ChecklistItem>> PostChecklistItem(ChecklistItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Texto))
            {
                return BadRequest(new { message = "Informe o texto do item." });
            }

            // A chave é sempre gerada aqui (nunca aceita do cliente) — é o valor gravado
            // no histórico de OS já finalizadas, então precisa ser estável e única.
            item.Chave = await GerarChaveUnicaAsync(item.Texto);
            item.IDChecklistItem = 0;

            _context.ChecklistItens.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChecklistItem), new { id = item.IDChecklistItem }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutChecklistItem(int id, ChecklistItem item)
        {
            if (id != item.IDChecklistItem)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(item.Texto))
            {
                return BadRequest(new { message = "Informe o texto do item." });
            }

            var existente = await _context.ChecklistItens.FindAsync(id);
            if (existente == null)
            {
                return NotFound();
            }

            // Chave não é editável — preserva a referência usada no histórico de OS.
            existente.Texto = item.Texto;
            existente.Ordem = item.Ordem;
            existente.Ativo = item.Ativo;
            existente.TiposClienteObrigatorio = NormalizarCsv(item.TiposClienteObrigatorio);
            existente.TiposPiscinaObrigatorio = NormalizarCsv(item.TiposPiscinaObrigatorio);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChecklistItem(int id)
        {
            var item = await _context.ChecklistItens.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.ChecklistItens.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static string? NormalizarCsv(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                return null;
            }

            var valores = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return valores.Length == 0 ? null : string.Join(",", valores);
        }

        private async Task<string> GerarChaveUnicaAsync(string texto)
        {
            var baseChave = Slugify(texto);
            if (string.IsNullOrEmpty(baseChave))
            {
                baseChave = "item";
            }

            var chave = baseChave;
            var sufixo = 1;
            while (await _context.ChecklistItens.AnyAsync(c => c.Chave == chave))
            {
                sufixo++;
                chave = $"{baseChave}-{sufixo}";
            }

            return chave;
        }

        private static string Slugify(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var semAcentos = new StringBuilder();
            foreach (var c in normalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    semAcentos.Append(c);
                }
            }

            var slug = semAcentos.ToString().ToLowerInvariant();
            slug = string.Concat(slug.Select(c => char.IsLetterOrDigit(c) ? c : ' '));
            var palavras = slug.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(5);
            slug = string.Join("-", palavras);

            return slug.Length > 50 ? slug[..50].TrimEnd('-') : slug;
        }
    }
}
