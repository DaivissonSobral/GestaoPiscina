using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    // Inscrição de um navegador/dispositivo para receber notificações Web Push (VAPID),
    // associada ao usuário logado no momento da inscrição. Nome "Registro" (em vez de
    // "PushSubscription") para não colidir com a classe PushSubscription do pacote WebPush.
    public class PushSubscriptionRegistro
    {
        public int IDPushSubscriptionRegistro { get; set; }

        public int IDUsuario { get; set; }

        [Required]
        [StringLength(500)]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string P256dh { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Auth { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public virtual Usuario Usuario { get; set; } = null!;
    }
}
