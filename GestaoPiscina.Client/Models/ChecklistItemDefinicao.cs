namespace GestaoPiscina.Client.Models
{
    // Item configurável do checklist operacional de uma OS (RN02) — não confundir com
    // Models.ChecklistItem, que é do checklist de conformidade (feature separada).
    public class ChecklistItemDefinicao
    {
        public int IDChecklistItem { get; set; }
        public string Chave { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public bool Ativo { get; set; } = true;
        public string? TiposClienteObrigatorio { get; set; }
        public string? TiposPiscinaObrigatorio { get; set; }

        // Auxiliares para a tela de cadastro (checkboxes) — não vão para a API
        // diretamente, só refletem/alimentam os campos CSV acima.
        public List<string> TiposClienteList
        {
            get => ParseCsv(TiposClienteObrigatorio);
            set => TiposClienteObrigatorio = ToCsv(value);
        }

        public List<string> TiposPiscinaList
        {
            get => ParseCsv(TiposPiscinaObrigatorio);
            set => TiposPiscinaObrigatorio = ToCsv(value);
        }

        // Uma OS exige este item se o tipo do cliente OU o tipo da piscina dessa OS
        // estiver numa das listas configuradas — vazio nas duas nunca é obrigatório.
        public bool AplicaSe(string? tipoCliente, string? tipoPiscina) =>
            (tipoCliente != null && TiposClienteList.Contains(tipoCliente)) ||
            (tipoPiscina != null && TiposPiscinaList.Contains(tipoPiscina));

        private static List<string> ParseCsv(string? csv) =>
            (csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        private static string? ToCsv(List<string> valores) =>
            valores.Count == 0 ? null : string.Join(",", valores);
    }
}
