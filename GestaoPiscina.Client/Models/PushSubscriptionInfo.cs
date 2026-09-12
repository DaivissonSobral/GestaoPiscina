namespace GestaoPiscina.Client.Models
{
    // Espelha o formato de PushSubscription.toJSON() do navegador (retornado por js/push.js).
    public class PushSubscriptionInfo
    {
        public string Endpoint { get; set; } = string.Empty;
        public PushSubscriptionKeysInfo Keys { get; set; } = new();
    }

    public class PushSubscriptionKeysInfo
    {
        public string P256dh { get; set; } = string.Empty;
        public string Auth { get; set; } = string.Empty;
    }
}
