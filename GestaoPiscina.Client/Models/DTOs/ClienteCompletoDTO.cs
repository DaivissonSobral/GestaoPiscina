using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models.DTOs
{
    public class ClienteCompletoDTO
    {
        // Dados do Cliente
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
        
        // Piscinas do Cliente
        public List<PiscinaDTO> Piscinas { get; set; } = new List<PiscinaDTO>();
        
        // Equipamentos do Cliente
        public List<EquipamentoDTO> Equipamentos { get; set; } = new List<EquipamentoDTO>();
        
        // Produtos (globais, não específicos do cliente)
        public List<ProdutoDTO> Produtos { get; set; } = new List<ProdutoDTO>();
    }

    public class PiscinaDTO
    {
        public int IDPiscina { get; set; }
        public int IDCliente { get; set; }
        
        [Required(ErrorMessage = "Tipo de piscina é obrigatório")]
        [StringLength(20, ErrorMessage = "Tipo deve ter no máximo 20 caracteres")]
        public string Tipo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Volume em litros é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Volume deve ser maior que zero")]
        public decimal VolumeLitros { get; set; }
        
        [StringLength(255, ErrorMessage = "Localização deve ter no máximo 255 caracteres")]
        public string? Localizacao { get; set; }
    }

    public class EquipamentoDTO
    {
        public int IDEquipamento { get; set; }
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
    }

    public class ProdutoDTO
    {
        public int IDProduto { get; set; }
        
        [Required(ErrorMessage = "Nome do produto é obrigatório")]
        public string Nome { get; set; } = string.Empty;
        
        public string? Concentracao { get; set; }
        
        [Required(ErrorMessage = "Unidade é obrigatória")]
        public string Unidade { get; set; } = string.Empty;
    }
} 