using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class Usuario
    {
        public int IDUsuario { get; set; }
        
        [Required]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Perfil { get; set; } = string.Empty; // gestor, técnico, supervisor, cliente, química
        
        [Required]
        [StringLength(50)]
        public string Login { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string SenhaHash { get; set; } = string.Empty;
    }
} 