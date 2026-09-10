using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class OrdemDeServico
    {
        public int IDOS { get; set; }
        
        public int IDPiscina { get; set; }
        
        [Required]
        public DateTime DataExecucao { get; set; }
        
        public bool ChecklistConcluido { get; set; }
        
        public string? Observacoes { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // Em Aberto, Finalizada, Ocorrência, Reagendada, Em Andamento, Cancelada

        public string? FotosAntes { get; set; } // URLs das fotos

        public string? FotosDepois { get; set; } // URLs das fotos

        public string? FotosOcorrencias { get; set; } // URLs das fotos

        public bool RelatorioGerado { get; set; }

        // Usuário responsável por autorizar a finalização por ocorrência (só se aplica quando Status = "Ocorrência")
        public int? Aprovador { get; set; }

        [Required]
        public int IDUsuario { get; set; } // Técnico responsável

        [Required]
        [Range(0, double.MaxValue)]
        public decimal pH { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Alcalinidade { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CloroLivre { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DurezaCalcica { get; set; }

        [Required]
        public DateTime HoraInicio { get; set; }

        [Required]
        public DateTime HoraTermino { get; set; }

        // Navegação
        public virtual Piscina Piscina { get; set; } = null!;
        public virtual Usuario Tecnico { get; set; } = null!;
        public virtual Usuario? AprovadorUsuario { get; set; }
    }
} 