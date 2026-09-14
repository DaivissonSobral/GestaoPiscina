namespace GestaoPiscina.Client.Models
{
    // Resultado bruto do JS interop de cálculo de rota (ver wwwroot/js/maps.js,
    // gestaoPiscinaMaps.calculateRoute) — usado por Pages/Rotas.razor pra desenhar a
    // rota otimizada entre o técnico e os clientes selecionados no mapa.
    public class RotaResultado
    {
        public bool Sucesso { get; set; }
        public double DistanciaMetros { get; set; }
        public double DuracaoSegundos { get; set; }

        // Índices (na mesma ordem em que os clientes selecionados foram enviados pro
        // JS) na ordem otimizada de visita sugerida pelo Google.
        public List<int>? Ordem { get; set; }
        public string? Erro { get; set; }
    }
}
