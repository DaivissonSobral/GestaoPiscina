using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Validation
{
    // Valida telefone brasileiro com DDD — 10 dígitos (fixo) ou 11 (celular, com o 9 na
    // frente). Campo vazio é considerado válido (quem exige o preenchimento usa [Required]
    // junto, à parte).
    public class TelefoneAttribute : ValidationAttribute
    {
        public TelefoneAttribute() : base("Telefone inválido. Informe o DDD e o número.")
        {
        }

        public override bool IsValid(object? value)
        {
            var texto = value as string;
            if (string.IsNullOrWhiteSpace(texto))
            {
                return true;
            }

            var digitos = new string(texto.Where(char.IsDigit).ToArray());
            return digitos.Length is 10 or 11;
        }
    }
}
