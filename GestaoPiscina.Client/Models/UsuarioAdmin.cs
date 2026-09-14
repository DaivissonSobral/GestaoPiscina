using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    // Usado na tela de administração de usuários (lista + criar/editar).
    // Espelha GestaoPiscina.Server.Controllers.UsuarioAdminDTO; SenhaInicial só é
    // usada ao criar um usuário novo (nunca é retornada pelo servidor).
    public class UsuarioAdmin
    {
        public int IDUsuario { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O login é obrigatório.")]
        [StringLength(20)]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione um perfil.")]
        public int IDPerfil { get; set; }

        public string Perfil { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; }

        public DateTime? UltimoAcesso { get; set; }

        [StringLength(500)]
        public string? FotoUrl { get; set; }

        // Obrigatório só para Técnico/Supervisor — checado manualmente em
        // UsuarioModal.HandleSubmit (não dá pra expressar "obrigatório condicional ao
        // perfil selecionado" com um [Required] simples), não pelo DataAnnotationsValidator.
        [StringLength(300)]
        public string? Endereco { get; set; }

        // Sem [StringLength(MinimumLength=...)] aqui de propósito: em modo de edição este
        // campo fica vazio (não é usado) e MinimumLength validaria a string vazia como
        // inválida mesmo sem [Required], bloqueando o submit do formulário de edição sem
        // nenhuma mensagem visível. O mínimo de 6 caracteres é validado manualmente em
        // UsuarioModal.HandleSubmit, só quando IDUsuario == 0 (criação).
        public string SenhaInicial { get; set; } = string.Empty;
    }
}
