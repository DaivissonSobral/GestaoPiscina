namespace GestaoPiscina.Server.Services
{
    // Centraliza o hash de senha em BCrypt (substitui o SHA-256 sem salt usado
    // antes em AuthController/SeedData — SHA-256 puro é vulnerável a rainbow
    // tables e não tem fator de custo ajustável).
    public static class PasswordHasher
    {
        public static string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

        public static bool Verify(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
    }
}
