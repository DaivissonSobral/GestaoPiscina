using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class Piscina
    {
        public int IDPiscina { get; set; }
        
        public int IDCliente { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty; // adulto, infantil ou espelho d'água
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal VolumeLitros { get; set; }
        
        [StringLength(255)]
        public string? Localizacao { get; set; }
        
        // Navegação
        public virtual Cliente Cliente { get; set; } = null!;
        public virtual ICollection<OrdemDeServico> OrdensDeServico { get; set; } = new List<OrdemDeServico>();
    }
} 