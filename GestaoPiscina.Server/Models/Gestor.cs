using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class Gestor
    {
        public int IDGestor { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Navegação
        public virtual ICollection<GestorCliente> GestorClientes { get; set; } = new List<GestorCliente>();
    }
}
