using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class Equipamento
    {
        public int IDEquipamento { get; set; }
        
        public int IDCliente { get; set; }
        
        [Required]
        [StringLength(100)]
        public string NumeroSerie { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;
        
        public DateTime? UltimaCalibragem { get; set; }
        
        public string? Observacao { get; set; }
        
        // Navegação
        public virtual Cliente Cliente { get; set; } = null!;
    }
} 