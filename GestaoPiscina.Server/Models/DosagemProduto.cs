using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    // Registra os produtos e quantidades aplicados durante uma OS, com base
    // na leitura de pH/cloro/alcalinidade/dureza feita na mesma OS. Ao ser
    // criada/editada/excluída, ajusta o estoque do cliente correspondente
    // (ver DosagensController).
    public class DosagemProduto
    {
        public int IDDosagem { get; set; }

        public int IDOS { get; set; }

        public int IDProduto { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public decimal Quantidade { get; set; }

        // Navegação
        public virtual OrdemDeServico OrdemDeServico { get; set; } = null!;
        public virtual Produto Produto { get; set; } = null!;
    }
}
