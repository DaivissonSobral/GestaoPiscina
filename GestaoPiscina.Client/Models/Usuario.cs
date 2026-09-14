namespace GestaoPiscina.Client.Models
{
    // Espelha GestaoPiscina.Server.Controllers.UsuarioResumoDTO (nunca inclui a senha).
    public class Usuario
    {
        public int IDUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
        // Usados pelo mapa de Gestão de Rota (ver Pages/Rotas.razor).
        public string? FotoUrl { get; set; }
        public string? Endereco { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
