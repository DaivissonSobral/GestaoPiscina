using System.Security.Cryptography;
using System.Text;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(GestaoPiscinaContext context)
        {
            // Verificar se já existem perfis
            if (!context.Perfis.Any())
            {
                var perfis = new List<Perfil>
                {
                    new Perfil
                    {
                        Nome = "Gestor",
                        Descricao = "Acesso total ao sistema",
                        PodeGerenciarUsuarios = true,
                        PodeGerenciarClientes = true,
                        PodeGerenciarPiscinas = true,
                        PodeGerenciarProdutos = true,
                        PodeGerenciarEstoque = true,
                        PodeGerenciarOrdensServico = true,
                        PodeGerenciarEquipamentos = true,
                        PodeVisualizarRelatorios = true,
                        PodeConfigurarSistema = true
                    },
                    new Perfil
                    {
                        Nome = "Supervisor",
                        Descricao = "Supervisão de operações",
                        PodeGerenciarUsuarios = false,
                        PodeGerenciarClientes = true,
                        PodeGerenciarPiscinas = true,
                        PodeGerenciarProdutos = true,
                        PodeGerenciarEstoque = true,
                        PodeGerenciarOrdensServico = true,
                        PodeGerenciarEquipamentos = true,
                        PodeVisualizarRelatorios = true,
                        PodeConfigurarSistema = false
                    },
                    new Perfil
                    {
                        Nome = "Técnico",
                        Descricao = "Execução de serviços",
                        PodeGerenciarUsuarios = false,
                        PodeGerenciarClientes = false,
                        PodeGerenciarPiscinas = false,
                        PodeGerenciarProdutos = false,
                        PodeGerenciarEstoque = false,
                        PodeGerenciarOrdensServico = true,
                        PodeGerenciarEquipamentos = false,
                        PodeVisualizarRelatorios = true,
                        PodeConfigurarSistema = false
                    },
                    new Perfil
                    {
                        Nome = "Cliente",
                        Descricao = "Acesso limitado para clientes",
                        PodeGerenciarUsuarios = false,
                        PodeGerenciarClientes = false,
                        PodeGerenciarPiscinas = false,
                        PodeGerenciarProdutos = false,
                        PodeGerenciarEstoque = false,
                        PodeGerenciarOrdensServico = false,
                        PodeGerenciarEquipamentos = false,
                        PodeVisualizarRelatorios = false,
                        PodeConfigurarSistema = false
                    }
                };

                context.Perfis.AddRange(perfis);
                await context.SaveChangesAsync();
            }

            // Verificar se já existem usuários
            if (!context.Usuarios.Any())
            {
                var gestorPerfil = context.Perfis.First(p => p.Nome == "Gestor");
                var supervisorPerfil = context.Perfis.First(p => p.Nome == "Supervisor");
                var tecnicoPerfil = context.Perfis.First(p => p.Nome == "Técnico");
                var clientePerfil = context.Perfis.First(p => p.Nome == "Cliente");

                var usuarios = new List<Usuario>
                {
                    new Usuario
                    {
                        Nome = "Administrador",
                        Email = "admin@gestaopiscina.com",
                        Login = "admin",
                        SenhaHash = HashPassword("123456"),
                        Ativo = true,
                        DataCriacao = DateTime.Now,
                        IDPerfil = gestorPerfil.IDPerfil
                    },
                    new Usuario
                    {
                        Nome = "João Silva",
                        Email = "joao.silva@gestaopiscina.com",
                        Login = "joao.silva",
                        SenhaHash = HashPassword("123456"),
                        Ativo = true,
                        DataCriacao = DateTime.Now,
                        IDPerfil = supervisorPerfil.IDPerfil
                    },
                    new Usuario
                    {
                        Nome = "Maria Santos",
                        Email = "maria.santos@gestaopiscina.com",
                        Login = "maria.santos",
                        SenhaHash = HashPassword("123456"),
                        Ativo = true,
                        DataCriacao = DateTime.Now,
                        IDPerfil = tecnicoPerfil.IDPerfil
                    },
                    new Usuario
                    {
                        Nome = "Cliente Teste",
                        Email = "cliente@teste.com",
                        Login = "cliente",
                        SenhaHash = HashPassword("123456"),
                        Ativo = true,
                        DataCriacao = DateTime.Now,
                        IDPerfil = clientePerfil.IDPerfil
                    }
                };

                context.Usuarios.AddRange(usuarios);
                await context.SaveChangesAsync();
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
} 