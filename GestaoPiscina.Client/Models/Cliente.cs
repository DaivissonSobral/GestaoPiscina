using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    public class Cliente
    {
        public int IDCliente { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo é obrigatório.")]
        public string Tipo { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O endereço é obrigatório.")]
        public string Endereco { get; set; } = string.Empty;

        public string? DiasDeVisita { get; set; }
        public string? Observacoes { get; set; }
        
        public List<Piscina> Piscinas { get; set; } = new List<Piscina>();
        public List<EstoqueCliente> Estoques { get; set; } = new List<EstoqueCliente>();
        public List<Equipamento> Equipamentos { get; set; } = new List<Equipamento>();
    }
} 