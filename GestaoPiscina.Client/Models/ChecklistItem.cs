namespace GestaoPiscina.Client.Models
{
    public class ChecklistItem
    {
        public string Section { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ExpectedPoints { get; set; }
        public int ObtainedPoints { get; set; }
        public bool Confirmed { get; set; }
        public string Status { get; set; } = ""; // "Aprovado" ou "Recusado"
        public string Observation { get; set; } = "";
    }
}