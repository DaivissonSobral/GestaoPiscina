using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    public class OrdemDeServico : IValidatableObject
    {
        public int IDOS { get; set; }

        [Required(ErrorMessage = "Piscina é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma piscina")]
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
        public string? FotosAntes { get; set; }

        [StringLength(500, ErrorMessage = "Fotos devem ter no máximo 500 caracteres")]
        public string? FotosDepois { get; set; }

        [StringLength(500, ErrorMessage = "Fotos devem ter no máximo 500 caracteres")]
        public string? FotosOcorrencias { get; set; }

        public bool RelatorioGerado { get; set; }

        public int? Aprovador { get; set; }

        [Required(ErrorMessage = "Técnico responsável é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione um técnico")]
        public int IDUsuario { get; set; }

        [Required(ErrorMessage = "pH é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "pH deve ser maior ou igual a zero")]
        public decimal pH { get; set; }

        [Required(ErrorMessage = "Alcalinidade é obrigatória")]
        [Range(0, double.MaxValue, ErrorMessage = "Alcalinidade deve ser maior ou igual a zero")]
        public decimal Alcalinidade { get; set; }

        [Required(ErrorMessage = "Cloro livre é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "Cloro livre deve ser maior ou igual a zero")]
        public decimal CloroLivre { get; set; }

        [Required(ErrorMessage = "Dureza cálcica é obrigatória")]
        [Range(0, double.MaxValue, ErrorMessage = "Dureza cálcica deve ser maior ou igual a zero")]
        public decimal DurezaCalcica { get; set; }

        [Required(ErrorMessage = "Horário de início é obrigatório")]
        public DateTime HoraInicio { get; set; }

        [Required(ErrorMessage = "Horário de término é obrigatório")]
        public DateTime HoraTermino { get; set; }

        public Piscina Piscina { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HoraTermino < HoraInicio)
            {
                yield return new ValidationResult(
                    "O horário de término não pode ser anterior ao horário de início.",
                    new[] { nameof(HoraTermino) });
            }

            if (Status == "Cancelada" && string.IsNullOrWhiteSpace(Observacoes))
            {
                yield return new ValidationResult(
                    "Observações são obrigatórias para cancelar a OS.",
                    new[] { nameof(Observacoes) });
            }
        }
    }
} 