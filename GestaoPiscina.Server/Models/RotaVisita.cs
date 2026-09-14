using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Server.Models
{
    // Uma parada confirmada na rota de um técnico num dia específico — usado pela tela de
    // Gestão de Rota (ver RotasController) pra guardar a sequência de visita decidida e
    // permitir consultar/acompanhar o andamento depois (comparando com o status das OS do
    // cliente na mesma data). Não duplica o técnico responsável — isso continua sendo o
    // IDUsuario de cada OrdemDeServico; esse registro só guarda a ordem de visita planejada.
    public class RotaVisita
    {
        public int IDRotaVisita { get; set; }

        [Required]
        public int IDUsuario { get; set; } // Técnico dono da rota

        [Required]
        public DateTime Data { get; set; } // Dia da rota (parte de data, sem hora)

        [Required]
        public int IDCliente { get; set; }

        [Required]
        public int Ordem { get; set; } // Posição (1-based) na sequência de visita do dia

        [Required]
        public DateTime DataConfirmacao { get; set; }

        // Navegação
        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Cliente Cliente { get; set; } = null!;
    }
}
