namespace GestaoPiscina.Client.Models
{
    // Resultado bruto do JS interop de geocodificação (ver wwwroot/js/maps.js,
    // gestaoPiscinaMaps.geocode) — usado ao salvar Cliente/Usuario pra converter o
    // endereço em texto em coordenadas antes de persistir.
    public class GeoResultado
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
