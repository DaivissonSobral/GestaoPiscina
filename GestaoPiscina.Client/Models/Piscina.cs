using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    public class Piscina
    {
        public int IDPiscina { get; set; }
        
        [Required(ErrorMessage = "ID do cliente é obrigatório")]
        public int IDCliente { get; set; }
        
        [Required(ErrorMessage = "Tipo de piscina é obrigatório")]
        [StringLength(20, ErrorMessage = "Tipo deve ter no máximo 20 caracteres")]
        public string Tipo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Volume em litros é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Volume deve ser maior que zero")]
        public decimal VolumeLitros { get; set; }
        
        [StringLength(255, ErrorMessage = "Localização deve ter no máximo 255 caracteres")]
        public string? Localizacao { get; set; }
        
        public Cliente Cliente { get; set; } = null!;
        public List<OrdemDeServico> OrdensDeServico { get; set; } = new List<OrdemDeServico>();
    }
} 