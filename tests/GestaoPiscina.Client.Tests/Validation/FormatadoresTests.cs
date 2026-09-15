using GestaoPiscina.Client.Validation;

namespace GestaoPiscina.Client.Tests.Validation
{
    public class FormatadoresTests
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("111", "111")]
        [InlineData("1114", "111.4")]
        [InlineData("11144477735", "111.444.777-35")]
        public void FormatarCpfCnpj_ComAte11Digitos_AplicaMascaraDeCpf(string? entrada, string esperado)
        {
            Assert.Equal(esperado, Formatadores.FormatarCpfCnpj(entrada));
        }

        [Theory]
        [InlineData("111444777350", "11.144.477/7350")]
        [InlineData("11222333000181", "11.222.333/0001-81")]
        public void FormatarCpfCnpj_ComMaisDe11Digitos_AplicaMascaraDeCnpj(string entrada, string esperado)
        {
            Assert.Equal(esperado, Formatadores.FormatarCpfCnpj(entrada));
        }

        [Fact]
        public void FormatarCpfCnpj_ComMaisDe14Digitos_TruncaEm14()
        {
            Assert.Equal("11.222.333/0001-81", Formatadores.FormatarCpfCnpj("112223330001819999"));
        }

        [Fact]
        public void FormatarCpfCnpj_IgnoraCaracteresQueNaoSaoDigitos()
        {
            Assert.Equal("111.444.777-35", Formatadores.FormatarCpfCnpj("111.444.777-35abc"));
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("8", "(8")]
        [InlineData("85", "(85")]
        [InlineData("859", "(85) 9")]
        public void FormatarTelefone_ProgressivamenteEnquantoDigita_MontaMascara(string? entrada, string esperado)
        {
            Assert.Equal(esperado, Formatadores.FormatarTelefone(entrada));
        }

        [Fact]
        public void FormatarTelefone_Com10Digitos_UsaMascaraDeFixo()
        {
            Assert.Equal("(85) 9811-1223", Formatadores.FormatarTelefone("8598111223"));
        }

        [Fact]
        public void FormatarTelefone_Com11Digitos_UsaMascaraDeCelular()
        {
            Assert.Equal("(85) 98111-2233", Formatadores.FormatarTelefone("85981112233"));
        }

        [Fact]
        public void FormatarTelefone_ComMaisDe11Digitos_TruncaEm11()
        {
            Assert.Equal("(85) 98111-2233", Formatadores.FormatarTelefone("859811122339999"));
        }

        [Fact]
        public void FormatarTelefone_ReformatarUmValorJaFormatado_MantemOMesmoResultado()
        {
            var formatado = Formatadores.FormatarTelefone("85981112233");

            Assert.Equal(formatado, Formatadores.FormatarTelefone(formatado));
        }
    }
}
