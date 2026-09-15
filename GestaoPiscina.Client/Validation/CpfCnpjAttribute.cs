using System.ComponentModel.DataAnnotations;

namespace GestaoPiscina.Client.Validation
{
    // Valida CPF (11 dígitos) ou CNPJ (14 dígitos) pelos dígitos verificadores oficiais —
    // qual dos dois é aplicado depende só da quantidade de dígitos informada. Campo vazio
    // é considerado válido (quem exige o preenchimento usa [Required] junto, à parte).
    public class CpfCnpjAttribute : ValidationAttribute
    {
        public CpfCnpjAttribute() : base("CNPJ ou CPF inválido.")
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

            return digitos.Length switch
            {
                11 => CpfValido(digitos),
                14 => CnpjValido(digitos),
                _ => false
            };
        }

        private static bool CpfValido(string cpf)
        {
            if (TodosDigitosIguais(cpf))
            {
                return false;
            }

            int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var digitoVerificador1 = CalcularDigitoVerificador(cpf[..9], multiplicadores1);
            var digitoVerificador2 = CalcularDigitoVerificador(cpf[..9] + digitoVerificador1, multiplicadores2);

            return cpf.EndsWith($"{digitoVerificador1}{digitoVerificador2}");
        }

        private static bool CnpjValido(string cnpj)
        {
            if (TodosDigitosIguais(cnpj))
            {
                return false;
            }

            int[] multiplicadores1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var digitoVerificador1 = CalcularDigitoVerificador(cnpj[..12], multiplicadores1);
            var digitoVerificador2 = CalcularDigitoVerificador(cnpj[..12] + digitoVerificador1, multiplicadores2);

            return cnpj.EndsWith($"{digitoVerificador1}{digitoVerificador2}");
        }

        private static int CalcularDigitoVerificador(string baseDigitos, int[] multiplicadores)
        {
            var soma = baseDigitos.Select((c, i) => (c - '0') * multiplicadores[i]).Sum();
            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        private static bool TodosDigitosIguais(string digitos) => digitos.Distinct().Count() == 1;
    }
}
