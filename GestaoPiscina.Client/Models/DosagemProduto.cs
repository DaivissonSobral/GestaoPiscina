using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Models
{
    public class DosagemProduto
    {
        public int IDDosagem { get; set; }

        public int IDOS { get; set; }

        [Required(ErrorMessage = "Selecione um produto")]
        public int IDProduto { get; set; }

        [Required(ErrorMessage = "Informe a quantidade")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public decimal Quantidade { get; set; }

        public Produto Produto { get; set; } = null!;
    }
}
