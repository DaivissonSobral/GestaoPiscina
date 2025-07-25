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
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Login { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string SenhaHash { get; set; } = string.Empty;
        
        public bool Ativo { get; set; } = true;
        
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        
        public DateTime? UltimoAcesso { get; set; }
        
        // Relacionamento com Perfil
        public int IDPerfil { get; set; }
        public Perfil Perfil { get; set; } = null!;
    }
} 