namespace GestaoPiscina.Server.Models
{
    public class GestorCliente
    {
        public int IDGestor { get; set; }
        public int IDCliente { get; set; }

        // Navegação
        public virtual Gestor Gestor { get; set; } = null!;
        public virtual Cliente Cliente { get; set; } = null!;
    }
}
