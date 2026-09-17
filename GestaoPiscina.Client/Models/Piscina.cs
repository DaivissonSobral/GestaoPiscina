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
        
        [Required(ErrorMessage = "Volume em m³ é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Volume deve ser maior que zero")]
        public decimal VolumeM3 { get; set; }
        
        [StringLength(255, ErrorMessage = "Localização deve ter no máximo 255 caracteres")]
        public string? Localizacao { get; set; }

        [Required(ErrorMessage = "Coberta é obrigatório")]
        public string Coberta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aquecida é obrigatório")]
        public bool Aquecida { get; set; }

        // Recorrência de manutenção (periodicidade de limpeza)
        public string RecorrenciaFrequencia { get; set; } = "Nenhuma"; // Nenhuma, Diaria, Semanal, Mensal
        public int RecorrenciaIntervalo { get; set; } = 1;
        public string? RecorrenciaDiasSemana { get; set; } // Ex: "Seg,Qua,Sex"
        public DateTime? RecorrenciaDataInicio { get; set; }
        public string RecorrenciaTermino { get; set; } = "Nunca"; // Nunca, Data, Ocorrencias
        public DateTime? RecorrenciaDataFim { get; set; }
        public int? RecorrenciaOcorrencias { get; set; }

        public Cliente Cliente { get; set; } = null!;
        public List<OrdemDeServico> OrdensDeServico { get; set; } = new List<OrdemDeServico>();
    }
} 