using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    // Lançamento no livro-razão de estoque de um cliente. O saldo atual de um par
    // (Cliente, Produto) nunca é armazenado diretamente: é sempre a soma de
    // Quantidade de todos os lançamentos daquele par (ver EstoqueCalculo).
    // Quantidade é sempre o delta assinado aplicado ao saldo (positivo soma,
    // negativo subtrai), independente do Tipo — isso permite SUM(Quantidade)
    // direto sem lógica condicional por tipo espalhada pelo código.
    public class MovimentacaoEstoque
    {
        public int IDMovimentacao { get; set; }

        public int IDCliente { get; set; }

        public int IDProduto { get; set; }

        // Entrada, Saida, Ajuste ou Inventario.
        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        public decimal Quantidade { get; set; }

        // Só preenchido quando Tipo == Inventario: o valor físico contado na
        // contagem (para auditoria — Quantidade, nesse caso, é a diferença entre
        // este valor e o saldo imediatamente anterior à contagem).
        public decimal? QuantidadeContada { get; set; }

        [Required]
        public DateTime Data { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? Observacao { get; set; }

        // Preenchido só quando a movimentação é uma Saída automática gerada por
        // uma dosagem de produto numa OS (ver DosagensController). Nulo para
        // lançamentos manuais feitos na tela de Estoque (Entrada/Ajuste/Inventário).
        public int? IDDosagem { get; set; }

        // Navegação
        public virtual Cliente Cliente { get; set; } = null!;
        public virtual Produto Produto { get; set; } = null!;
        public virtual DosagemProduto? Dosagem { get; set; }
    }
}
