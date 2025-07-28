using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models.DTOs
{
    public class ClienteCompletoDTO
    {
        // Dados do Cliente
        public int IDCliente { get; set; }
        
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(150, ErrorMessage = "Nome deve ter no máximo 150 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo é obrigatório.")]
        [StringLength(20, ErrorMessage = "Tipo deve ter no máximo 20 caracteres")]
        public string Tipo { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
        public string Telefone { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O endereço é obrigatório.")]
        [StringLength(255, ErrorMessage = "Endereço deve ter no máximo 255 caracteres")]
        public string Endereco { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Dias de visita deve ter no máximo 50 caracteres")]
        public string? DiasDeVisita { get; set; }
        
        public string? Observacoes { get; set; }
        
        // Piscinas do Cliente
        public List<PiscinaDTO> Piscinas { get; set; } = new List<PiscinaDTO>();
        
        // Equipamentos do Cliente
        public List<EquipamentoDTO> Equipamentos { get; set; } = new List<EquipamentoDTO>();
        
        // Estoque do Cliente (produtos específicos do cliente)
        public List<EstoqueClienteDTO> Estoques { get; set; } = new List<EstoqueClienteDTO>();
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

    public class EstoqueClienteDTO
    {
        public int IDEstoque { get; set; }
        public int IDCliente { get; set; }
        public int IDProduto { get; set; }
        
        [Required(ErrorMessage = "Quantidade atual é obrigatória")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantidade deve ser maior ou igual a zero")]
        public decimal QuantidadeAtual { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Quantidade mínima deve ser maior ou igual a zero")]
        public decimal? QuantidadeMinima { get; set; }
        
        // Dados do produto para facilitar o frontend
        [Required(ErrorMessage = "Nome do produto é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome do produto deve ter no máximo 100 caracteres")]
        public string NomeProduto { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Concentração deve ter no máximo 50 caracteres")]
        public string? ConcentracaoProduto { get; set; }
        
        [Required(ErrorMessage = "Unidade do produto é obrigatória")]
        [StringLength(10, ErrorMessage = "Unidade deve ter no máximo 10 caracteres")]
        public string UnidadeProduto { get; set; } = string.Empty;
    }
} 