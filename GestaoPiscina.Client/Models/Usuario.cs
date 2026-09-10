namespace GestaoPiscina.Client.Models
{
    // Espelha GestaoPiscina.Server.Controllers.UsuarioResumoDTO (nunca inclui a senha).
    public class Usuario
    {
        public int IDUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
    }
}
