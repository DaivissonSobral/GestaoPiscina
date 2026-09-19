using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistItens",
                columns: table => new
                {
                    IDChecklistItem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Chave = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    TiposClienteObrigatorio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TiposPiscinaObrigatorio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItens", x => x.IDChecklistItem);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    IDCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Endereco = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DiasDeVisita = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CNPJ = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.IDCliente);
                });

            migrationBuilder.CreateTable(
                name: "Gestores",
                columns: table => new
                {
                    IDGestor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gestores", x => x.IDGestor);
                });

            migrationBuilder.CreateTable(
                name: "Perfis",
                columns: table => new
                {
                    IDPerfil = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PodeGerenciarUsuarios = table.Column<bool>(type: "bit", nullable: false),
                    PodeGerenciarClientes = table.Column<bool>(type: "bit", nullable: false),
                    PodeGerenciarPiscinas = table.Column<bool>(type: "bit", nullable: false),
                    PodeGerenciarProdutos = table.Column<bool>(type: "bit", nullable: false),
                    PodeGerenciarEstoque = table.Column<bool>(type: "bit", nullable: false),
                    PodeGerenciarOrdensServico = table.Column<bool>(type: "bit", nullable: false),
                    PodeGerenciarEquipamentos = table.Column<bool>(type: "bit", nullable: false),
                    PodeVisualizarRelatorios = table.Column<bool>(type: "bit", nullable: false),
                    PodeConfigurarSistema = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.IDPerfil);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    IDProduto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Concentracao = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Unidade = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.IDProduto);
                });

            migrationBuilder.CreateTable(
                name: "Equipamentos",
                columns: table => new
                {
                    IDEquipamento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDCliente = table.Column<int>(type: "int", nullable: false),
                    NumeroSerie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UltimaCalibragem = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipamentos", x => x.IDEquipamento);
                    table.ForeignKey(
                        name: "FK_Equipamentos_Clientes_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Clientes",
                        principalColumn: "IDCliente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Piscinas",
                columns: table => new
                {
                    IDPiscina = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDCliente = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VolumeM3 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Localizacao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Coberta = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Aquecida = table.Column<bool>(type: "bit", nullable: false),
                    RecorrenciaFrequencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecorrenciaIntervalo = table.Column<int>(type: "int", nullable: false),
                    RecorrenciaDiasSemana = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecorrenciaDataInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecorrenciaTermino = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    RecorrenciaDataFim = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecorrenciaOcorrencias = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Piscinas", x => x.IDPiscina);
                    table.ForeignKey(
                        name: "FK_Piscinas_Clientes_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Clientes",
                        principalColumn: "IDCliente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GestorClientes",
                columns: table => new
                {
                    IDGestor = table.Column<int>(type: "int", nullable: false),
                    IDCliente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GestorClientes", x => new { x.IDGestor, x.IDCliente });
                    table.ForeignKey(
                        name: "FK_GestorClientes_Clientes_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Clientes",
                        principalColumn: "IDCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GestorClientes_Gestores_IDGestor",
                        column: x => x.IDGestor,
                        principalTable: "Gestores",
                        principalColumn: "IDGestor",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IDUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Login = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoAcesso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    IDPerfil = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IDUsuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Perfis_IDPerfil",
                        column: x => x.IDPerfil,
                        principalTable: "Perfis",
                        principalColumn: "IDPerfil",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EstoqueClientes",
                columns: table => new
                {
                    IDEstoque = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDCliente = table.Column<int>(type: "int", nullable: false),
                    IDProduto = table.Column<int>(type: "int", nullable: false),
                    QuantidadeMinima = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstoqueClientes", x => x.IDEstoque);
                    table.ForeignKey(
                        name: "FK_EstoqueClientes_Clientes_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Clientes",
                        principalColumn: "IDCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstoqueClientes_Produtos_IDProduto",
                        column: x => x.IDProduto,
                        principalTable: "Produtos",
                        principalColumn: "IDProduto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdensDeServico",
                columns: table => new
                {
                    IDOS = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDPiscina = table.Column<int>(type: "int", nullable: false),
                    DataExecucao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChecklistConcluido = table.Column<bool>(type: "bit", nullable: false),
                    ChecklistItens = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FotosAntes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FotosDepois = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FotosOcorrencias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatorioGerado = table.Column<bool>(type: "bit", nullable: false),
                    Aprovador = table.Column<int>(type: "int", nullable: true),
                    OcorrenciaAprovada = table.Column<bool>(type: "bit", nullable: false),
                    DataAprovacaoOcorrencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OcorrenciaReprovada = table.Column<bool>(type: "bit", nullable: false),
                    DataReprovacaoOcorrencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IDUsuario = table.Column<int>(type: "int", nullable: true),
                    LatitudeInicio = table.Column<double>(type: "float", nullable: true),
                    LongitudeInicio = table.Column<double>(type: "float", nullable: true),
                    FotoHodometro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InicioPercurso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    pH = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Alcalinidade = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    CloroLivre = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    DurezaCalcica = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    pHDepois = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    AlcalinidadeDepois = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    CloroLivreDepois = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    DurezaCalcicaDepois = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    HoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraTermino = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensDeServico", x => x.IDOS);
                    table.ForeignKey(
                        name: "FK_OrdensDeServico_Piscinas_IDPiscina",
                        column: x => x.IDPiscina,
                        principalTable: "Piscinas",
                        principalColumn: "IDPiscina",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdensDeServico_Usuarios_Aprovador",
                        column: x => x.Aprovador,
                        principalTable: "Usuarios",
                        principalColumn: "IDUsuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdensDeServico_Usuarios_IDUsuario",
                        column: x => x.IDUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IDUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushSubscriptionRegistros",
                columns: table => new
                {
                    IDPushSubscriptionRegistro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDUsuario = table.Column<int>(type: "int", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    P256dh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Auth = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscriptionRegistros", x => x.IDPushSubscriptionRegistro);
                    table.ForeignKey(
                        name: "FK_PushSubscriptionRegistros_Usuarios_IDUsuario",
                        column: x => x.IDUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IDUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RotaVisitas",
                columns: table => new
                {
                    IDRotaVisita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDUsuario = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IDCliente = table.Column<int>(type: "int", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    DataConfirmacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RotaVisitas", x => x.IDRotaVisita);
                    table.ForeignKey(
                        name: "FK_RotaVisitas_Clientes_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Clientes",
                        principalColumn: "IDCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RotaVisitas_Usuarios_IDUsuario",
                        column: x => x.IDUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IDUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DosagensProdutos",
                columns: table => new
                {
                    IDDosagem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDOS = table.Column<int>(type: "int", nullable: false),
                    IDProduto = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DosagensProdutos", x => x.IDDosagem);
                    table.ForeignKey(
                        name: "FK_DosagensProdutos_OrdensDeServico_IDOS",
                        column: x => x.IDOS,
                        principalTable: "OrdensDeServico",
                        principalColumn: "IDOS",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DosagensProdutos_Produtos_IDProduto",
                        column: x => x.IDProduto,
                        principalTable: "Produtos",
                        principalColumn: "IDProduto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    IDMovimentacao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDCliente = table.Column<int>(type: "int", nullable: false),
                    IDProduto = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    QuantidadeContada = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IDDosagem = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.IDMovimentacao);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Clientes_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Clientes",
                        principalColumn: "IDCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_DosagensProdutos_IDDosagem",
                        column: x => x.IDDosagem,
                        principalTable: "DosagensProdutos",
                        principalColumn: "IDDosagem");
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Produtos_IDProduto",
                        column: x => x.IDProduto,
                        principalTable: "Produtos",
                        principalColumn: "IDProduto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItens_Chave",
                table: "ChecklistItens",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DosagensProdutos_IDOS",
                table: "DosagensProdutos",
                column: "IDOS");

            migrationBuilder.CreateIndex(
                name: "IX_DosagensProdutos_IDProduto",
                table: "DosagensProdutos",
                column: "IDProduto");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_IDCliente",
                table: "Equipamentos",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueClientes_IDCliente",
                table: "EstoqueClientes",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_EstoqueClientes_IDProduto",
                table: "EstoqueClientes",
                column: "IDProduto");

            migrationBuilder.CreateIndex(
                name: "IX_GestorClientes_IDCliente",
                table: "GestorClientes",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Gestores_Email",
                table: "Gestores",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_IDCliente_IDProduto",
                table: "MovimentacoesEstoque",
                columns: new[] { "IDCliente", "IDProduto" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_IDDosagem",
                table: "MovimentacoesEstoque",
                column: "IDDosagem");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_IDProduto",
                table: "MovimentacoesEstoque",
                column: "IDProduto");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensDeServico_Aprovador",
                table: "OrdensDeServico",
                column: "Aprovador");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensDeServico_IDPiscina",
                table: "OrdensDeServico",
                column: "IDPiscina");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensDeServico_IDUsuario",
                table: "OrdensDeServico",
                column: "IDUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Piscinas_IDCliente",
                table: "Piscinas",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptionRegistros_Endpoint",
                table: "PushSubscriptionRegistros",
                column: "Endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptionRegistros_IDUsuario",
                table: "PushSubscriptionRegistros",
                column: "IDUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_RotaVisitas_IDCliente",
                table: "RotaVisitas",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_RotaVisitas_IDUsuario_Data_IDCliente",
                table: "RotaVisitas",
                columns: new[] { "IDUsuario", "Data", "IDCliente" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IDPerfil",
                table: "Usuarios",
                column: "IDPerfil");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistItens");

            migrationBuilder.DropTable(
                name: "Equipamentos");

            migrationBuilder.DropTable(
                name: "EstoqueClientes");

            migrationBuilder.DropTable(
                name: "GestorClientes");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "PushSubscriptionRegistros");

            migrationBuilder.DropTable(
                name: "RotaVisitas");

            migrationBuilder.DropTable(
                name: "Gestores");

            migrationBuilder.DropTable(
                name: "DosagensProdutos");

            migrationBuilder.DropTable(
                name: "OrdensDeServico");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Piscinas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Perfis");
        }
    }
}
