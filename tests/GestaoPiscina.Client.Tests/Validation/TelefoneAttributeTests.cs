using GestaoPiscina.Client.Validation;

namespace GestaoPiscina.Client.Tests.Validation
{
    public class TelefoneAttributeTests
    {
        private readonly TelefoneAttribute atributo = new();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IsValid_ComCampoVazio_RetornaTrue(string? valor)
        {
            Assert.True(atributo.IsValid(valor));
        }

        [Theory]
        [InlineData("(85) 3111-2233")]
        [InlineData("8531112233")]
        public void IsValid_ComTelefoneFixoDe10Digitos_RetornaTrue(string telefone)
        {
            Assert.True(atributo.IsValid(telefone));
        }

        [Theory]
        [InlineData("(85) 98111-2233")]
        [InlineData("85981112233")]
        public void IsValid_ComCelularDe11Digitos_RetornaTrue(string telefone)
        {
            Assert.True(atributo.IsValid(telefone));
        }

        [Theory]
        [InlineData("123")]
        [InlineData("123456789")]
        [InlineData("123456789012")]
        public void IsValid_ComQuantidadeDeDigitosDiferenteDe10Ou11_RetornaFalse(string telefone)
        {
            Assert.False(atributo.IsValid(telefone));
        }
    }
}
