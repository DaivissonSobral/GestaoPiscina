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

            // Adicionar dados de teste para debug
            if (!context.Clientes.Any())
            {
                var clientes = new List<Cliente>
                {
                    new Cliente
                    {
                        Nome = "Cliente Teste 1",
                        Tipo = "Residencial Privativa",
                        Endereco = "Rua Teste 1, 123",
                        Telefone = "(11) 99999-9999",
                        Email = "cliente1@teste.com",
                        DiasDeVisita = "Segunda, Quarta, Sexta",
                        Observacoes = "Cliente de teste para debug"
                    },
                    new Cliente
                    {
                        Nome = "Cliente Teste 2",
                        Tipo = "Uso Coletivo",
                        Endereco = "Rua Teste 2, 456",
                        Telefone = "(11) 88888-8888",
                        Email = "cliente2@teste.com",
                        DiasDeVisita = "Terça, Quinta",
                        Observacoes = "Cliente de teste para debug"
                    }
                };

                context.Clientes.AddRange(clientes);
                await context.SaveChangesAsync();
            }

            // Catálogo de produtos
            if (!context.Produtos.Any())
            {
                var produtos = new List<Produto>
                {
                    new Produto { Nome = "Hipoclorito de Sódio 10-12%", Concentracao = 10.0m, Unidade = "L" },
                    new Produto { Nome = "Dicloro 56%", Concentracao = 56.0m, Unidade = "kg" },
                    new Produto { Nome = "Hipoclorito de Cálcio 65%", Concentracao = 65.0m, Unidade = "kg" },
                    new Produto { Nome = "Pastilhas de Tricloro", Unidade = "kg" },
                    new Produto { Nome = "Bicarbonato de Sódio", Unidade = "kg" },
                    new Produto { Nome = "Limpa Bordas", Unidade = "L" },
                    new Produto { Nome = "Algicida Choque", Unidade = "L" },
                    new Produto { Nome = "Clarificante", Unidade = "L" },
                    new Produto { Nome = "Gel (Auxiliar de Filtração)", Unidade = "L" },
                    new Produto { Nome = "Barrilha", Unidade = "kg" },
                    new Produto { Nome = "Sulfato de Alumínio", Unidade = "kg" },
                    new Produto { Nome = "Elevador de Dureza Cálcica", Unidade = "kg" },
                    new Produto { Nome = "Ácido Muriático", Unidade = "L" },
                    new Produto { Nome = "Eliminador de Oleosidade", Unidade = "L" }
                };

                context.Produtos.AddRange(produtos);
                await context.SaveChangesAsync();
            }

            // Adicionar estoque de teste
            if (!context.EstoqueClientes.Any())
            {
                var clientes = context.Clientes.ToList();
                var produtos = context.Produtos.ToList();
                
                if (clientes.Any() && produtos.Any())
                {
                    var cliente1 = clientes.FirstOrDefault(c => c.Nome == "Cliente Teste 1");
                    var cliente2 = clientes.FirstOrDefault(c => c.Nome == "Cliente Teste 2");
                    var cloro = produtos.FirstOrDefault(p => p.Nome == "Hipoclorito de Sódio 10-12%");
                    var alcalinidade = produtos.FirstOrDefault(p => p.Nome == "Barrilha");
                    var phMais = produtos.FirstOrDefault(p => p.Nome == "Ácido Muriático");

                    var estoques = new List<EstoqueCliente>();

                    if (cliente1 != null && cloro != null)
                    {
                        estoques.Add(new EstoqueCliente
                        {
                            IDCliente = cliente1.IDCliente,
                            IDProduto = cloro.IDProduto,
                            QuantidadeAtual = 5.0m,
                            QuantidadeMinima = 1.0m
                        });
                    }

                    if (cliente1 != null && alcalinidade != null)
                    {
                        estoques.Add(new EstoqueCliente
                        {
                            IDCliente = cliente1.IDCliente,
                            IDProduto = alcalinidade.IDProduto,
                            QuantidadeAtual = 3.0m,
                            QuantidadeMinima = 0.5m
                        });
                    }

                    if (cliente2 != null && phMais != null)
                    {
                        estoques.Add(new EstoqueCliente
                        {
                            IDCliente = cliente2.IDCliente,
                            IDProduto = phMais.IDProduto,
                            QuantidadeAtual = 2.0m,
                            QuantidadeMinima = 1.0m
                        });
                    }

                    if (estoques.Any())
                    {
                        context.EstoqueClientes.AddRange(estoques);
                        await context.SaveChangesAsync();
                    }
                }
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