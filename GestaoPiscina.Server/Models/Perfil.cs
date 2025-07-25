using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class Perfil
    {
        public int IDPerfil { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? Descricao { get; set; }
        
        // Propriedades de permissão
        public bool PodeGerenciarUsuarios { get; set; }
        public bool PodeGerenciarClientes { get; set; }
        public bool PodeGerenciarPiscinas { get; set; }
        public bool PodeGerenciarProdutos { get; set; }
        public bool PodeGerenciarEstoque { get; set; }
        public bool PodeGerenciarOrdensServico { get; set; }
        public bool PodeGerenciarEquipamentos { get; set; }
        public bool PodeVisualizarRelatorios { get; set; }
        public bool PodeConfigurarSistema { get; set; }
    }
} 