namespace GestaoPiscina.Client.Models
{
    // Espelha GestaoPiscina.Server.Controllers.RotasController.RotaTecnicoDTO/VisitaDTO —
    // rota confirmada de um técnico num dia (ver Pages/Rotas.razor).
    public class RotaTecnico
    {
        public int IDUsuario { get; set; }
        public DateTime Data { get; set; }
        public DateTime DataConfirmacao { get; set; }
        public List<VisitaRota> Visitas { get; set; } = new();
    }

    public class VisitaRota
    {
        public int IDCliente { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public bool Concluida { get; set; }
    }
}
