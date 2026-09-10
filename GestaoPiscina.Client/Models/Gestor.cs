using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    public class Gestor
    {
        public int IDGestor { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(150, ErrorMessage = "Nome deve ter no máximo 150 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public List<GestorCliente> GestorClientes { get; set; } = new List<GestorCliente>();
    }

    public class GestorCliente
    {
        public int IDGestor { get; set; }
        public int IDCliente { get; set; }
        public Cliente? Cliente { get; set; }
    }
}
