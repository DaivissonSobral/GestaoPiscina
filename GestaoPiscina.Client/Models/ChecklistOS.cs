using System.Globalization;

namespace GestaoPiscina.Client.Models
{
    // Itens fixos do checklist de atendimento e montagem do texto dinâmico de produtos
    // aplicados — compartilhado entre o formulário de edição (OrdemServicoModal) e a
    // Visualização (OrdemServicoDetalhes) para não duplicar a lista/lógica nos dois.
    public static class ChecklistOS
    {
        public static readonly (string Chave, string Texto)[] ItensFixos = new[]
        {
            ("cesto", "Realizamos a limpeza do cesto pré-filtro."),
            ("aspiracao", "Foi realizada a aspiração filtrando, isso gera economia de água para o estabelecimento."),
            ("bordas", "Realizamos a limpeza das bordas."),
            ("material", "Foi realizada a retirada de material suspenso."),
            ("retrolavagem", "Realizamos a retrolavagem do filtro."),
        };

        public static HashSet<string> ItensMarcados(string? checklistItens) =>
            (checklistItens ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        public static string NomePiscina(string? tipo)
        {
            if (string.IsNullOrEmpty(tipo))
            {
                return "A piscina";
            }

            return $"Piscina {char.ToUpper(tipo[0])}{tipo[1..]}";
        }

        // Monta a frase dinâmica de produtos aplicados, ex.:
        // "Piscina Infantil: foi adicionado 1,8kg de dicloro e 8L de redutor de pH."
        public static string TextoDosagemAplicada(string nomePiscina, IReadOnlyCollection<DosagemProduto> dosagens)
        {
            if (dosagens.Count == 0)
            {
                return "";
            }

            var partes = dosagens
                .Select(d => $"{d.Quantidade.ToString("0.##", CultureInfo.InvariantCulture)}{d.Produto?.Unidade} de {d.Produto?.Nome}")
                .ToList();

            var descricao = partes.Count == 1
                ? partes[0]
                : string.Join(", ", partes.Take(partes.Count - 1)) + " e " + partes[^1];

            var verbo = partes.Count == 1 ? "foi adicionado" : "foram adicionados";
            return $"{nomePiscina}: {verbo} {descricao}.";
        }
    }
}
