using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class Produto
    {
        public int IDProduto { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;
        
        [Range(0, double.MaxValue)]
        public decimal? Concentracao { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Unidade { get; set; } = string.Empty;
        
        // Navegação
        public virtual ICollection<EstoqueCliente> Estoques { get; set; } = new List<EstoqueCliente>();
    }
} 