using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif" };
        private static readonly string[] PastasPermitidas = { "os", "usuarios" };
        private const long TamanhoMaximoBytes = 10 * 1024 * 1024; // 10MB

        public UploadsController(BlobServiceClient blobServiceClient, IConfiguration config)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = config["AzureStorage:ContainerName"] ?? "uploads";
        }

        [HttpPost("foto")]
        [RequestSizeLimit(TamanhoMaximoBytes)]
        public async Task<ActionResult<object>> UploadFoto(IFormFile arquivo, [FromQuery] string pasta = "os")
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

            if (!PastasPermitidas.Contains(pasta))
            {
                return BadRequest(new { message = "Destino de upload inválido." });
            }

            var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            var blobClient = containerClient.GetBlobClient($"{pasta}/{nomeArquivo}");

            using (var stream = arquivo.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = arquivo.ContentType });
            }

            return Ok(new { url = blobClient.Uri.ToString() });
        }
    }
}
