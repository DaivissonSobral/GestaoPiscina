using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class Produto
    {
        public int IDProduto { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Concentracao { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Unidade { get; set; } = string.Empty;
        
        // Navegação
        public virtual ICollection<EstoqueCliente> Estoques { get; set; } = new List<EstoqueCliente>();
    }
} 