using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    public class MovimentacaoEstoque
    {
        public int IDMovimentacao { get; set; }

        public int IDCliente { get; set; }

        public int IDProduto { get; set; }

        [Required(ErrorMessage = "Selecione o tipo de movimentação")]
        public string Tipo { get; set; } = string.Empty;

        public decimal Quantidade { get; set; }

        public decimal? QuantidadeContada { get; set; }

        [Required(ErrorMessage = "Informe a data")]
        public DateTime Data { get; set; } = DateTime.Today;

        [StringLength(255, ErrorMessage = "Observação deve ter no máximo 255 caracteres")]
        public string? Observacao { get; set; }

        public int? IDDosagem { get; set; }

        public Produto? Produto { get; set; }
    }
}
