using GestaoPiscina.Server.Services;
using Xunit;

namespace GestaoPiscina.Server.Tests.Services
{
    // RNF04 – Segurança e Autenticação: login/senha criptografados.
    public class PasswordHasherTests
    {
        [Fact]
        public void Verify_ComSenhaCorreta_RetornaTrue()
        {
            var hash = PasswordHasher.Hash("MinhaSenha123!");

            Assert.True(PasswordHasher.Verify("MinhaSenha123!", hash));
        }

        [Fact]
        public void Verify_ComSenhaIncorreta_RetornaFalse()
        {
            var hash = PasswordHasher.Hash("MinhaSenha123!");

            Assert.False(PasswordHasher.Verify("SenhaErrada", hash));
        }

        [Fact]
        public void Hash_NuncaArmazenaASenhaEmTextoPlano()
        {
            var senha = "MinhaSenha123!";

            var hash = PasswordHasher.Hash(senha);

            Assert.DoesNotContain(senha, hash);
        }

        [Fact]
        public void Hash_GeraHashesDiferentesParaAMesmaSenha()
        {
            // BCrypt usa salt aleatório por chamada — dois hashes da mesma senha não
            // devem ser iguais, mesmo que ambos validem contra ela.
            var senha = "MinhaSenha123!";

            var hash1 = PasswordHasher.Hash(senha);
            var hash2 = PasswordHasher.Hash(senha);

            Assert.NotEqual(hash1, hash2);
            Assert.True(PasswordHasher.Verify(senha, hash1));
            Assert.True(PasswordHasher.Verify(senha, hash2));
        }
    }
}
