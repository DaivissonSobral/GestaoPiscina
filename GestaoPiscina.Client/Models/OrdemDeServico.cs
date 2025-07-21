namespace GestaoPiscina.Client.Models
{
    public class OrdemDeServico
    {
        public int IDOS { get; set; }
        public int IDPiscina { get; set; }
        public DateTime DataExecucao { get; set; }
        public bool ChecklistConcluido { get; set; }
        public string? Observacoes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? FotosAntesDepois { get; set; }
        public bool RelatorioGerado { get; set; }
        
        public Piscina Piscina { get; set; } = null!;
    }
} 