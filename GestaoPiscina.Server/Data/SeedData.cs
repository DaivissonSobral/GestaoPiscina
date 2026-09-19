using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;

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
                        PodeConfigurarSistema = false,
                        ExigeEndereco = true
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
                        PodeConfigurarSistema = false,
                        ExigeEndereco = true
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
                    },
                    new Perfil
                    {
                        // Papel distinto de Técnico: valida as fórmulas de dosagem e assina
                        // digitalmente o relatório mensal (Especificação de Requisitos v4).
                        Nome = "Química",
                        Descricao = "Responsável técnica pela qualidade da água e assinatura dos relatórios",
                        PodeGerenciarUsuarios = false,
                        PodeGerenciarClientes = false,
                        PodeGerenciarPiscinas = false,
                        PodeGerenciarProdutos = false,
                        PodeGerenciarEstoque = false,
                        PodeGerenciarOrdensServico = false,
                        PodeGerenciarEquipamentos = false,
                        PodeVisualizarRelatorios = true,
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
                var quimicaPerfil = context.Perfis.First(p => p.Nome == "Química");

                var usuarios = new List<Usuario>
                {
                    new Usuario
                    {
                        Nome = "Administrador",
                        Email = "admin@gestaopiscina.com",
                        Login = "admin",
                        SenhaHash = PasswordHasher.Hash("123456"),
                        Ativo = true,
                        DataCriacao = DateTime.Now,
                        IDPerfil = gestorPerfil.IDPerfil
                    },
                    new Usuario
                    {
                        Nome = "João Silva",
                        Email = "joao.silva@gestaopiscina.com",
                        Login = "joao.silva",
                        SenhaHash = PasswordHasher.Hash("123456"),
                        Ativo = true,
                        DataCriacao = DateTime.Now,
                        IDPerfil = supervisorPerfil.IDPerfil
                    },
                    new Usuario
                    {
                        Nome = "Maria Santos",
                        Email = "maria.santos@gestaopiscina.com",
                        Login = "maria.santos",
                        SenhaHash = PasswordHasher.Hash("123456"),
                        Ativo = true,
                        DataCriacao = DateTime.Now,
                        IDPerfil = tecnicoPerfil.IDPerfil
                    },
                    new Usuario
                    {
                        Nome = "Cliente Teste",
                        Email = "cliente@teste.com",
                        Login = "cliente",
                        SenhaHash = PasswordHasher.Hash("123456"),
                        Ativo = true,
                        DataCriacao = DateTime.Now,
                        IDPerfil = clientePerfil.IDPerfil
                    },
                    new Usuario
                    {
                        // Mesma responsável técnica hoje referenciada nos relatórios
                        // (assinatura fixa em RelatorioOS.razor/RelatorioOSIndividual.razor).
                        Nome = "Talina da Silva Ferreira dos Santos",
                        Email = "talina.santos@gestaopiscina.com",
                        Login = "talina.santos",
                        SenhaHash = PasswordHasher.Hash("123456"),
                        Ativo = true,
                        DataCriacao = DateTime.Now,
                        IDPerfil = quimicaPerfil.IDPerfil
                    }
                };

                context.Usuarios.AddRange(usuarios);
                await context.SaveChangesAsync();
            }

            // Migração para bancos já existentes (os blocos acima só rodam em banco vazio):
            // adiciona o perfil Química e a usuária correspondente se ainda não existirem, e
            // reidrata para BCrypt qualquer SenhaHash que ainda esteja no formato SHA-256
            // antigo (hashes BCrypt sempre começam com "$2").
            if (!context.Perfis.Any(p => p.Nome == "Química"))
            {
                context.Perfis.Add(new Perfil
                {
                    Nome = "Química",
                    Descricao = "Responsável técnica pela qualidade da água e assinatura dos relatórios",
                    PodeGerenciarUsuarios = false,
                    PodeGerenciarClientes = false,
                    PodeGerenciarPiscinas = false,
                    PodeGerenciarProdutos = false,
                    PodeGerenciarEstoque = false,
                    PodeGerenciarOrdensServico = false,
                    PodeGerenciarEquipamentos = false,
                    PodeVisualizarRelatorios = true,
                    PodeConfigurarSistema = false
                });
                await context.SaveChangesAsync();
            }

            // Checa por Login OU Email: se o usuário fosse editado depois (ex.: login
            // trocado na tela de Usuários, mantendo o mesmo e-mail), checar só o Login
            // deixava passar e violava a constraint única de Email ao tentar recriar.
            if (!context.Usuarios.Any(u => u.Login == "talina.santos" || u.Email == "talina.santos@gestaopiscina.com"))
            {
                var quimicaPerfilExistente = context.Perfis.First(p => p.Nome == "Química");
                context.Usuarios.Add(new Usuario
                {
                    Nome = "Talina da Silva Ferreira dos Santos",
                    Email = "talina.santos@gestaopiscina.com",
                    Login = "talina.santos",
                    SenhaHash = PasswordHasher.Hash("123456"),
                    Ativo = true,
                    DataCriacao = DateTime.Now,
                    IDPerfil = quimicaPerfilExistente.IDPerfil
                });
                await context.SaveChangesAsync();
            }

            // Backfill do flag ExigeEndereco para bancos provisionados antes dele existir
            // (a coluna nasce com default false via migração; aqui ligamos para os perfis
            // que atendem no endereço do cliente: Piscineiro/Técnico de Operações, Líder,
            // Técnico e Supervisor — nomeados no banco como "Técnico em Manutenção",
            // "Líder de Operações", "Técnico" e "Supervisor").
            var nomesPerfisComEnderecoObrigatorio = new[] { "Técnico", "Supervisor", "Líder de Operações", "Técnico em Manutenção" };
            var perfisQueExigemEndereco = context.Perfis
                .Where(p => nomesPerfisComEnderecoObrigatorio.Contains(p.Nome) && !p.ExigeEndereco)
                .ToList();
            if (perfisQueExigemEndereco.Any())
            {
                foreach (var perfil in perfisQueExigemEndereco)
                {
                    perfil.ExigeEndereco = true;
                }
                await context.SaveChangesAsync();
            }

            var usuariosComHashAntigo = context.Usuarios.Where(u => !u.SenhaHash.StartsWith("$2")).ToList();
            if (usuariosComHashAntigo.Any())
            {
                foreach (var usuario in usuariosComHashAntigo)
                {
                    usuario.SenhaHash = PasswordHasher.Hash("123456");
                }
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

                    // (Cliente, Produto, quantidade inicial, quantidade mínima) — a
                    // quantidade inicial vira uma Entrada no livro-razão, já que o saldo
                    // não é mais um campo direto de EstoqueCliente.
                    var itens = new List<(Cliente Cliente, Produto Produto, decimal QuantidadeInicial, decimal QuantidadeMinima)>();

                    if (cliente1 != null && cloro != null)
                    {
                        itens.Add((cliente1, cloro, 5.0m, 1.0m));
                    }

                    if (cliente1 != null && alcalinidade != null)
                    {
                        itens.Add((cliente1, alcalinidade, 3.0m, 0.5m));
                    }

                    if (cliente2 != null && phMais != null)
                    {
                        itens.Add((cliente2, phMais, 2.0m, 1.0m));
                    }

                    if (itens.Any())
                    {
                        foreach (var item in itens)
                        {
                            var estoque = new EstoqueCliente
                            {
                                IDCliente = item.Cliente.IDCliente,
                                IDProduto = item.Produto.IDProduto,
                                QuantidadeMinima = item.QuantidadeMinima
                            };
                            context.EstoqueClientes.Add(estoque);
                            context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                            {
                                IDCliente = item.Cliente.IDCliente,
                                IDProduto = item.Produto.IDProduto,
                                Tipo = "Entrada",
                                Quantidade = item.QuantidadeInicial,
                                Data = DateTime.Now,
                                Observacao = "Estoque inicial (seed)"
                            });
                        }

                        await context.SaveChangesAsync();
                    }
                }
            }
        }

    }
} 