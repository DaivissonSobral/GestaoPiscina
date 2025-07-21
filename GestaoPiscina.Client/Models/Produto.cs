namespace GestaoPiscina.Client.Models
{
    public class Produto
    {
        public int IDProduto { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Concentracao { get; set; }
        public string Unidade { get; set; } = string.Empty;
        
        public List<EstoqueCliente> Estoques { get; set; } = new List<EstoqueCliente>();
    }
} 