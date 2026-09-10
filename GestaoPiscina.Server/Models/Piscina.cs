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

        [Required]
        [StringLength(10)]
        public string Coberta { get; set; } = string.Empty; // Sim, Não ou Parcial

        [Required]
        public bool Aquecida { get; set; }

        // Recorrência de manutenção (periodicidade de limpeza)
        [Required]
        [StringLength(20)]
        public string RecorrenciaFrequencia { get; set; } = "Nenhuma"; // Nenhuma, Diaria, Semanal, Mensal

        [Range(1, 365)]
        public int RecorrenciaIntervalo { get; set; } = 1; // "a cada N dias/semanas/meses"

        [StringLength(50)]
        public string? RecorrenciaDiasSemana { get; set; } // Ex: "Seg,Qua,Sex" (apenas quando Semanal)

        public DateTime? RecorrenciaDataInicio { get; set; }

        [Required]
        [StringLength(15)]
        public string RecorrenciaTermino { get; set; } = "Nunca"; // Nunca, Data, Ocorrencias

        public DateTime? RecorrenciaDataFim { get; set; } // usado quando RecorrenciaTermino == "Data"

        public int? RecorrenciaOcorrencias { get; set; } // usado quando RecorrenciaTermino == "Ocorrencias"

        // Navegação
        public virtual Cliente Cliente { get; set; } = null!;
        public virtual ICollection<OrdemDeServico> OrdensDeServico { get; set; } = new List<OrdemDeServico>();
    }
} 