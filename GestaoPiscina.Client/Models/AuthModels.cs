using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Login é obrigatório")]
        public string Login { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; } = string.Empty;
    }
    
    public class LoginResponse
    {
        public bool Sucesso { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiraEm { get; set; }
        public string? Mensagem { get; set; }
        public UsuarioInfo? Usuario { get; set; }
    }
    
    public class UsuarioInfo
    {
        public int IDUsuario { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string NomePerfil { get; set; } = string.Empty;
        public int IDPerfil { get; set; }
        public Dictionary<string, bool> Permissoes { get; set; } = new();
    }
    
    public class AlterarSenhaRequest
    {
        [Required(ErrorMessage = "Senha atual é obrigatória")]
        public string SenhaAtual { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
        public string NovaSenha { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("NovaSenha", ErrorMessage = "Senhas não conferem")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
} 