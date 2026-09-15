using GestaoPiscina.Client.Validation;

namespace GestaoPiscina.Client.Tests.Validation
{
    public class CpfCnpjAttributeTests
    {
        private readonly CpfCnpjAttribute atributo = new();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IsValid_ComCampoVazio_RetornaTrue(string? valor)
        {
            Assert.True(atributo.IsValid(valor));
        }

        [Theory]
        [InlineData("111.444.777-35")]
        [InlineData("11144477735")]
        public void IsValid_ComCpfValido_RetornaTrue(string cpf)
        {
            Assert.True(atributo.IsValid(cpf));
        }

        [Theory]
        [InlineData("111.444.777-36")]
        [InlineData("111.111.111-11")]
        [InlineData("000.000.000-00")]
        public void IsValid_ComCpfInvalido_RetornaFalse(string cpf)
        {
            Assert.False(atributo.IsValid(cpf));
        }

        [Theory]
        [InlineData("11.222.333/0001-81")]
        [InlineData("11222333000181")]
        public void IsValid_ComCnpjValido_RetornaTrue(string cnpj)
        {
            Assert.True(atributo.IsValid(cnpj));
        }

        [Theory]
        [InlineData("11.222.333/0001-80")]
        [InlineData("11.111.111/1111-11")]
        public void IsValid_ComCnpjInvalido_RetornaFalse(string cnpj)
        {
            Assert.False(atributo.IsValid(cnpj));
        }

        [Theory]
        [InlineData("123")]
        [InlineData("123456789012")]
        [InlineData("123456789012345")]
        public void IsValid_ComQuantidadeDeDigitosDiferenteDe11Ou14_RetornaFalse(string valor)
        {
            Assert.False(atributo.IsValid(valor));
        }
    }
}
