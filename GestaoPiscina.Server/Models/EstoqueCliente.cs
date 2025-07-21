using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class EstoqueCliente
    {
        public int IDEstoque { get; set; }
        
        public int IDCliente { get; set; }
        
        public int IDProduto { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal QuantidadeAtual { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? QuantidadeMinima { get; set; }
        
        // Navegação
        public virtual Cliente Cliente { get; set; } = null!;
        public virtual Produto Produto { get; set; } = null!;
    }
} 