using System.Text;

namespace GestaoPiscina.Client.Validation
{
    // Máscaras aplicadas enquanto o usuário digita (ver uso com InputText/ValueChanged nos
    // formulários). Reutilizável em qualquer tela que precise formatar CPF/CNPJ ou telefone.
    public static class Formatadores
    {
        public static string FormatarCpfCnpj(string? valor)
        {
            var digitos = SomenteDigitos(valor, 14);
            return digitos.Length <= 11 ? FormatarCpf(digitos) : FormatarCnpj(digitos);
        }

        public static string FormatarTelefone(string? valor)
        {
            var digitos = SomenteDigitos(valor, 11);
            if (digitos.Length == 0)
            {
                return "";
            }

            var posicaoHifen = digitos.Length == 11 ? 7 : 6;
            var sb = new StringBuilder("(");
            for (var i = 0; i < digitos.Length; i++)
            {
                if (i == 2)
                {
                    sb.Append(") ");
                }
                else if (i == posicaoHifen)
                {
                    sb.Append('-');
                }

                sb.Append(digitos[i]);
            }

            return sb.ToString();
        }

        private static string FormatarCpf(string digitos)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < digitos.Length; i++)
            {
                if (i == 3 || i == 6)
                {
                    sb.Append('.');
                }
                else if (i == 9)
                {
                    sb.Append('-');
                }

                sb.Append(digitos[i]);
            }

            return sb.ToString();
        }

        private static string FormatarCnpj(string digitos)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < digitos.Length; i++)
            {
                if (i == 2 || i == 5)
                {
                    sb.Append('.');
                }
                else if (i == 8)
                {
                    sb.Append('/');
                }
                else if (i == 12)
                {
                    sb.Append('-');
                }

                sb.Append(digitos[i]);
            }

            return sb.ToString();
        }

        private static string SomenteDigitos(string? valor, int tamanhoMaximo)
        {
            var digitos = new string((valor ?? "").Where(char.IsDigit).ToArray());
            return digitos.Length > tamanhoMaximo ? digitos[..tamanhoMaximo] : digitos;
        }
    }
}
