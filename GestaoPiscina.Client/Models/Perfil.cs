namespace GestaoPiscina.Client.Models
{
    public class Perfil
    {
        public int IDPerfil { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool ExigeEndereco { get; set; }
    }
}
