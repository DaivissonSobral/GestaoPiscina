using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationAndProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Perfil",
                table: "Usuarios");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Usuarios",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IDPerfil",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoAcesso",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Perfis",
                columns: table => new
                {
                    IDPerfil = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PodeGerenciarUsuarios = table.Column<bool>(type: "INTEGER", nullable: false),
                    PodeGerenciarClientes = table.Column<bool>(type: "INTEGER", nullable: false),
                    PodeGerenciarPiscinas = table.Column<bool>(type: "INTEGER", nullable: false),
                    PodeGerenciarProdutos = table.Column<bool>(type: "INTEGER", nullable: false),
                    PodeGerenciarEstoque = table.Column<bool>(type: "INTEGER", nullable: false),
                    PodeGerenciarOrdensServico = table.Column<bool>(type: "INTEGER", nullable: false),
                    PodeGerenciarEquipamentos = table.Column<bool>(type: "INTEGER", nullable: false),
                    PodeVisualizarRelatorios = table.Column<bool>(type: "INTEGER", nullable: false),
                    PodeConfigurarSistema = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.IDPerfil);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IDPerfil",
                table: "Usuarios",
                column: "IDPerfil");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Perfis_IDPerfil",
                table: "Usuarios",
                column: "IDPerfil",
                principalTable: "Perfis",
                principalColumn: "IDPerfil",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Perfis_IDPerfil",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Perfis");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_IDPerfil",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IDPerfil",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UltimoAcesso",
                table: "Usuarios");

            migrationBuilder.AddColumn<string>(
                name: "Perfil",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
