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
                name: "Clientes",
                columns: table => new
                {
                    IDCliente = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DiasDeVisita = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.IDCliente);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    IDProduto = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Concentracao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Unidade = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.IDProduto);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IDUsuario = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Perfil = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Login = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IDUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Equipamentos",
                columns: table => new
                {
                    IDEquipamento = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDCliente = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroSerie = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UltimaCalibragem = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observacao = table.Column<string>(type: "TEXT", nullable: true)
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
                    IDPiscina = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDCliente = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    VolumeLitros = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Localizacao = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
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
                name: "EstoquesCliente",
                columns: table => new
                {
                    IDEstoque = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDCliente = table.Column<int>(type: "INTEGER", nullable: false),
                    IDProduto = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeAtual = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    QuantidadeMinima = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstoquesCliente", x => x.IDEstoque);
                    table.ForeignKey(
                        name: "FK_EstoquesCliente_Clientes_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Clientes",
                        principalColumn: "IDCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstoquesCliente_Produtos_IDProduto",
                        column: x => x.IDProduto,
                        principalTable: "Produtos",
                        principalColumn: "IDProduto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdensDeServico",
                columns: table => new
                {
                    IDOS = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDPiscina = table.Column<int>(type: "INTEGER", nullable: false),
                    DataExecucao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChecklistConcluido = table.Column<bool>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FotosAntesDepois = table.Column<string>(type: "TEXT", nullable: true),
                    RelatorioGerado = table.Column<bool>(type: "INTEGER", nullable: false)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_IDCliente",
                table: "Equipamentos",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_EstoquesCliente_IDCliente",
                table: "EstoquesCliente",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_EstoquesCliente_IDProduto",
                table: "EstoquesCliente",
                column: "IDProduto");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensDeServico_IDPiscina",
                table: "OrdensDeServico",
                column: "IDPiscina");

            migrationBuilder.CreateIndex(
                name: "IX_Piscinas_IDCliente",
                table: "Piscinas",
                column: "IDCliente");

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
                name: "Equipamentos");

            migrationBuilder.DropTable(
                name: "EstoquesCliente");

            migrationBuilder.DropTable(
                name: "OrdensDeServico");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Piscinas");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
