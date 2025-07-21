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
        public string Status { get; set; } = string.Empty; // Em Aberto, Finalizada, Reagendada
        
        public string? FotosAntesDepois { get; set; } // URLs das fotos
        
        public bool RelatorioGerado { get; set; }
        
        // Navegação
        public virtual Piscina Piscina { get; set; } = null!;
    }
} 