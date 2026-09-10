using Microsoft.AspNetCore.Mvc;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif" };
        private const long TamanhoMaximoBytes = 10 * 1024 * 1024; // 10MB

        public UploadsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("foto")]
        [RequestSizeLimit(TamanhoMaximoBytes)]
        public async Task<ActionResult<object>> UploadFoto(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                return BadRequest(new { message = "Nenhum arquivo enviado." });
            }

            if (arquivo.Length > TamanhoMaximoBytes)
            {
                return BadRequest(new { message = "A foto deve ter no máximo 10MB." });
            }

            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            if (!ExtensoesPermitidas.Contains(extensao))
            {
                return BadRequest(new { message = "Formato de imagem não suportado. Envie JPG, PNG, WEBP ou HEIC." });
            }

            var pastaUploads = Path.Combine(_env.WebRootPath, "uploads", "os");
            Directory.CreateDirectory(pastaUploads);

            var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
            var caminhoCompleto = Path.Combine(pastaUploads, nomeArquivo);

            using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            var urlAbsoluta = $"{Request.Scheme}://{Request.Host}/uploads/os/{nomeArquivo}";
            return Ok(new { url = urlAbsoluta });
        }
    }
}
