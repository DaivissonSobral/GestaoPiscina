using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    public class OrdemDeServico
    {
        public int IDOS { get; set; }
        
        [Required(ErrorMessage = "Piscina é obrigatória")]
        public int IDPiscina { get; set; }
        
        [Required(ErrorMessage = "Data de execução é obrigatória")]
        public DateTime DataExecucao { get; set; }
        
        public bool ChecklistConcluido { get; set; }
        
        [StringLength(1000, ErrorMessage = "Observações devem ter no máximo 1000 caracteres")]
        public string? Observacoes { get; set; }
        
        [Required(ErrorMessage = "Status é obrigatório")]
        [StringLength(20, ErrorMessage = "Status deve ter no máximo 20 caracteres")]
        public string Status { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Fotos devem ter no máximo 500 caracteres")]
        public string? FotosAntesDepois { get; set; }
        
        public bool RelatorioGerado { get; set; }
        
        public Piscina Piscina { get; set; } = null!;
    }
} 