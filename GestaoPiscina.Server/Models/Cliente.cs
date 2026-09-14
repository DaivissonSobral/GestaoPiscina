using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class Cliente
    {
        public int IDCliente { get; set; }
        
        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;
        
        [Required]
        [StringLength(30)]
        public string Tipo { get; set; } = string.Empty; // Uso Coletivo ou Residencial Privativa
        
        [Required]
        [StringLength(255)]
        public string Endereco { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? DiasDeVisita { get; set; }
        
        public string? Observacoes { get; set; }
        
        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(18)]
        public string? CNPJ { get; set; }

        // Preenchidos via geocodificação (no navegador, ver wwwroot/js/maps.js) sempre que
        // o Endereco é salvo — usados para posicionar o cliente no mapa de Gestão de Rota,
        // sem precisar geocodificar de novo a cada carregamento do mapa.
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Navegação
        public virtual ICollection<Piscina> Piscinas { get; set; } = new List<Piscina>();
        public virtual ICollection<EstoqueCliente> Estoques { get; set; } = new List<EstoqueCliente>();
        public virtual ICollection<Equipamento> Equipamentos { get; set; } = new List<Equipamento>();
        public virtual ICollection<GestorCliente> GestorClientes { get; set; } = new List<GestorCliente>();
    }
} 