namespace GestaoPiscina.Client.Models
{
    public class Equipamento
    {
        public int IDEquipamento { get; set; }
        public int IDCliente { get; set; }
        public string NumeroSerie { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime? UltimaCalibragem { get; set; }
        public string? Observacao { get; set; }
        
        public Cliente Cliente { get; set; } = null!;
    }
} 