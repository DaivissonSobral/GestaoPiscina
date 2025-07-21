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
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty; // condomínio ou residência
        
        [Required]
        [StringLength(255)]
        public string Endereco { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? DiasDeVisita { get; set; }
        
        public string? Observacoes { get; set; }
        
        // Navegação
        public virtual ICollection<Piscina> Piscinas { get; set; } = new List<Piscina>();
        public virtual ICollection<EstoqueCliente> Estoques { get; set; } = new List<EstoqueCliente>();
        public virtual ICollection<Equipamento> Equipamentos { get; set; } = new List<Equipamento>();
    }
} 