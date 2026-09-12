using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    // Item configurável do checklist operacional de uma OS (RN02). A obrigatoriedade
    // é definida por listas de tipos de cliente e de piscina: o item é obrigatório
    // numa OS se o tipo do cliente OU o tipo da piscina daquela OS estiver em uma das
    // listas — ambas vazias significa item sempre opcional.
    public class ChecklistItem
    {
        public int IDChecklistItem { get; set; }

        // Identificador estável gerado pelo servidor a partir do texto na criação (nunca
        // reeditado depois) — é o valor gravado no campo OrdemDeServico.ChecklistItens,
        // então trocar essa chave quebraria o histórico de OS já marcadas. Sem [Required]
        // de propósito: o cliente sempre envia vazio (ChecklistItensController a gera),
        // e o [ApiController] rejeitaria a requisição automaticamente antes do controller
        // rodar se essa validação estivesse ativa.
        [StringLength(60)]
        public string Chave { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Texto { get; set; } = string.Empty;

        public int Ordem { get; set; }

        public bool Ativo { get; set; } = true;

        // CSV com os valores de Cliente.Tipo (ex.: "Uso Coletivo,Residencial Privativa").
        [StringLength(100)]
        public string? TiposClienteObrigatorio { get; set; }

        // CSV com os valores de Piscina.Tipo (ex.: "adulto,infantil,espelho").
        [StringLength(100)]
        public string? TiposPiscinaObrigatorio { get; set; }
    }
}
