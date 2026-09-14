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

        [StringLength(500)]
        public string? FotoUrl { get; set; }

        // Obrigatório apenas para Técnico e Supervisor (ver ValidarEndereco em
        // UsuariosController) — os demais perfis não dependem de deslocamento até o cliente.
        [StringLength(300)]
        public string? Endereco { get; set; }

        // Preenchidos via geocodificação (no navegador) sempre que o Endereco é salvo —
        // usados para posicionar o técnico/supervisor no mapa de Gestão de Rota.
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Relacionamento com Perfil
        public int IDPerfil { get; set; }
        public Perfil Perfil { get; set; } = null!;
    }
} 