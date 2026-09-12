using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PushSubscriptionsController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;
        private readonly IConfiguration _configuration;

        public PushSubscriptionsController(GestaoPiscinaContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Chave pública VAPID, usada pelo navegador em pushManager.subscribe(). Não é
        // segredo — só a chave privada (usada só no servidor para assinar) precisa ser protegida.
        [HttpGet("vapid-public-key")]
        public ActionResult<string> GetVapidPublicKey()
        {
            var publicKey = _configuration["WebPush:PublicKey"];
            if (string.IsNullOrEmpty(publicKey))
            {
                return NotFound();
            }

            return Ok(publicKey);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostSubscription(PushSubscriptionRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var idUsuario))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Endpoint) || request.Keys == null
                || string.IsNullOrWhiteSpace(request.Keys.P256dh) || string.IsNullOrWhiteSpace(request.Keys.Auth))
            {
                return BadRequest(new { message = "Inscrição de push inválida." });
            }

            var existente = await _context.PushSubscriptionRegistros
                .FirstOrDefaultAsync(p => p.Endpoint == request.Endpoint);

            if (existente != null)
            {
                existente.IDUsuario = idUsuario;
                existente.P256dh = request.Keys.P256dh;
                existente.Auth = request.Keys.Auth;
            }
            else
            {
                _context.PushSubscriptionRegistros.Add(new PushSubscriptionRegistro
                {
                    IDUsuario = idUsuario,
                    Endpoint = request.Endpoint,
                    P256dh = request.Keys.P256dh,
                    Auth = request.Keys.Auth
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    // Espelha o formato de PushSubscription.toJSON() do navegador.
    public class PushSubscriptionRequest
    {
        public string Endpoint { get; set; } = string.Empty;
        public PushSubscriptionKeys? Keys { get; set; }

        public class PushSubscriptionKeys
        {
            public string P256dh { get; set; } = string.Empty;
            public string Auth { get; set; } = string.Empty;
        }
    }
}
