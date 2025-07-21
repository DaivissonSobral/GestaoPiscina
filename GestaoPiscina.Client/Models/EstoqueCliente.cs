namespace GestaoPiscina.Client.Models
{
    public class EstoqueCliente
    {
        public int IDEstoque { get; set; }
        public int IDCliente { get; set; }
        public int IDProduto { get; set; }
        public decimal QuantidadeAtual { get; set; }
        public decimal? QuantidadeMinima { get; set; }
        
        public Cliente Cliente { get; set; } = null!;
        public Produto Produto { get; set; } = null!;
    }
} 