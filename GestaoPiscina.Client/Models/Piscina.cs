namespace GestaoPiscina.Client.Models
{
    public class Piscina
    {
        public int IDPiscina { get; set; }
        public int IDCliente { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal VolumeLitros { get; set; }
        public string? Localizacao { get; set; }
        
        public Cliente Cliente { get; set; } = null!;
        public List<OrdemDeServico> OrdensDeServico { get; set; } = new List<OrdemDeServico>();
    }
} 