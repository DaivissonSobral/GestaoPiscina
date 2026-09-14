using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    public class OrdemDeServico
    {
        public int IDOS { get; set; }
        
        public int IDPiscina { get; set; }
        
        [Required]
        public DateTime DataExecucao { get; set; }
        
        public bool ChecklistConcluido { get; set; }

        // Chaves (separadas por vírgula) dos itens fixos de checklist marcados como concluídos
        // nesta OS. ChecklistConcluido é derivado no cliente: todos os itens marcados + pelo
        // menos uma dosagem de produto registrada.
        public string? ChecklistItens { get; set; }

        public string? Observacoes { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // Em Aberto, Finalizada, Ocorrência, Reagendada, Em Andamento, Cancelada

        public string? FotosAntes { get; set; } // URLs das fotos

        public string? FotosDepois { get; set; } // URLs das fotos

        public string? FotosOcorrencias { get; set; } // URLs das fotos

        public bool RelatorioGerado { get; set; }

        // Usuário responsável por autorizar a finalização por ocorrência (só se aplica quando Status = "Ocorrência")
        public int? Aprovador { get; set; }

        // Aprovação posterior da ocorrência pelo químico responsável (Aprovador) — separada
        // do registro inicial, feita depois via endpoint próprio (ver aprovar-ocorrencia).
        public bool OcorrenciaAprovada { get; set; }

        public DateTime? DataAprovacaoOcorrencia { get; set; }

        [Required]
        public int IDUsuario { get; set; } // Técnico responsável

        // Localização exata do técnico (geolocalização do navegador/celular) no momento em
        // que ele clicou "Iniciar" nesta OS — usada pra traçar a rota até o cliente na tela
        // de cadastro/detalhes (ver RotaOSWidget no cliente). Diferente de Usuario.Latitude/
        // Longitude (endereço cadastrado do técnico), que é só uma estimativa de partida.
        public double? LatitudeInicio { get; set; }
        public double? LongitudeInicio { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal pH { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Alcalinidade { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CloroLivre { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DurezaCalcica { get; set; }

        [Required]
        public DateTime HoraInicio { get; set; }

        [Required]
        public DateTime HoraTermino { get; set; }

        // Navegação
        public virtual Piscina Piscina { get; set; } = null!;
        public virtual Usuario Tecnico { get; set; } = null!;
        public virtual Usuario? AprovadorUsuario { get; set; }
        public virtual ICollection<DosagemProduto> Dosagens { get; set; } = new List<DosagemProduto>();
    }
} 