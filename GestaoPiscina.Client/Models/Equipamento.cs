using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    public class Equipamento
    {
        public int IDEquipamento { get; set; }
        
        [Required(ErrorMessage = "ID do cliente é obrigatório")]
        public int IDCliente { get; set; }
        
        [Required(ErrorMessage = "Número de série é obrigatório")]
        [StringLength(100, ErrorMessage = "Número de série deve ter no máximo 100 caracteres")]
        public string NumeroSerie { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tipo de equipamento é obrigatório")]
        [StringLength(50, ErrorMessage = "Tipo deve ter no máximo 50 caracteres")]
        public string Tipo { get; set; } = string.Empty;
        
        public DateTime? UltimaCalibragem { get; set; }
        
        [StringLength(500, ErrorMessage = "Observação deve ter no máximo 500 caracteres")]
        public string? Observacao { get; set; }
        
        public Cliente Cliente { get; set; } = null!;
    }
} 