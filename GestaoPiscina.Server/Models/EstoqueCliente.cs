using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoPiscina.Server.Models
{
    // Representa apenas "este cliente acompanha este produto no estoque, com este
    // limite mínimo" — não guarda mais o saldo diretamente. O saldo atual é sempre
    // calculado a partir da soma dos lançamentos em MovimentacaoEstoque (ver
    // EstoqueCalculo), e populado em QuantidadeAtual só na resposta da API.
    public class EstoqueCliente
    {
        public int IDEstoque { get; set; }

        public int IDCliente { get; set; }

        public int IDProduto { get; set; }

        // Não mapeado no banco: calculado e preenchido pelos controllers a partir
        // de MovimentacaoEstoque antes de serializar a resposta.
        [NotMapped]
        public decimal QuantidadeAtual { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? QuantidadeMinima { get; set; }

        // Navegação
        public virtual Cliente Cliente { get; set; } = null!;
        public virtual Produto Produto { get; set; } = null!;
    }
} 