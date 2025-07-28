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
                        Tipo = "Residencial",
                        Endereco = "Rua Teste 1, 123",
                        Telefone = "(11) 99999-9999",
                        Email = "cliente1@teste.com",
                        DiasDeVisita = "Segunda, Quarta, Sexta",
                        Observacoes = "Cliente de teste para debug"
                    },
                    new Cliente
                    {
                        Nome = "Cliente Teste 2",
                        Tipo = "Comercial",
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

            // Adicionar produtos de teste
            if (!context.Produtos.Any())
            {
                var produtos = new List<Produto>
                {
                    new Produto
                    {
                        Nome = "Cloro",
                        Concentracao = "65%",
                        Unidade = "kg"
                    },
                    new Produto
                    {
                        Nome = "Alcalinidade",
                        Concentracao = "100%",
                        Unidade = "kg"
                    },
                    new Produto
                    {
                        Nome = "pH+",
                        Concentracao = "100%",
                        Unidade = "kg"
                    },
                    new Produto
                    {
                        Nome = "pH-",
                        Concentracao = "100%",
                        Unidade = "kg"
                    }
                };

                context.Produtos.AddRange(produtos);
                await context.SaveChangesAsync();
            }

            // Adicionar estoque de teste
            if (!context.EstoquesCliente.Any())
            {
                var clientes = context.Clientes.ToList();
                var produtos = context.Produtos.ToList();
                
                if (clientes.Any() && produtos.Any())
                {
                    var cliente1 = clientes.FirstOrDefault(c => c.Nome == "Cliente Teste 1");
                    var cliente2 = clientes.FirstOrDefault(c => c.Nome == "Cliente Teste 2");
                    var cloro = produtos.FirstOrDefault(p => p.Nome == "Cloro");
                    var alcalinidade = produtos.FirstOrDefault(p => p.Nome == "Alcalinidade");
                    var phMais = produtos.FirstOrDefault(p => p.Nome == "pH+");

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
                        context.EstoquesCliente.AddRange(estoques);
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