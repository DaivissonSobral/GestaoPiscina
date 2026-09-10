using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AlinharComRequisitosV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FotosAntesDepois",
                table: "OrdensDeServico",
                newName: "FotosOcorrencias");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Equipamentos",
                newName: "Descricao");

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Aquecida",
                table: "Piscinas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Coberta",
                table: "Piscinas",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Alcalinidade",
                table: "OrdensDeServico",
                type: "TEXT",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Aprovador",
                table: "OrdensDeServico",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CloroLivre",
                table: "OrdensDeServico",
                type: "TEXT",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DurezaCalcica",
                table: "OrdensDeServico",
                type: "TEXT",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FotosAntes",
                table: "OrdensDeServico",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotosDepois",
                table: "OrdensDeServico",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HoraInicio",
                table: "OrdensDeServico",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "HoraTermino",
                table: "OrdensDeServico",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IDUsuario",
                table: "OrdensDeServico",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "pH",
                table: "OrdensDeServico",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Gestores",
                columns: table => new
                {
                    IDGestor = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gestores", x => x.IDGestor);
                });

            migrationBuilder.CreateTable(
                name: "GestorClientes",
                columns: table => new
                {
                    IDGestor = table.Column<int>(type: "INTEGER", nullable: false),
                    IDCliente = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_OrdensDeServico_Aprovador",
                table: "OrdensDeServico",
                column: "Aprovador");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensDeServico_IDUsuario",
                table: "OrdensDeServico",
                column: "IDUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_GestorClientes_IDCliente",
                table: "GestorClientes",
                column: "IDCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Gestores_Email",
                table: "Gestores",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensDeServico_Usuarios_Aprovador",
                table: "OrdensDeServico",
                column: "Aprovador",
                principalTable: "Usuarios",
                principalColumn: "IDUsuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensDeServico_Usuarios_IDUsuario",
                table: "OrdensDeServico",
                column: "IDUsuario",
                principalTable: "Usuarios",
                principalColumn: "IDUsuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdensDeServico_Usuarios_Aprovador",
                table: "OrdensDeServico");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdensDeServico_Usuarios_IDUsuario",
                table: "OrdensDeServico");

            migrationBuilder.DropTable(
                name: "GestorClientes");

            migrationBuilder.DropTable(
                name: "Gestores");

            migrationBuilder.DropIndex(
                name: "IX_OrdensDeServico_Aprovador",
                table: "OrdensDeServico");

            migrationBuilder.DropIndex(
                name: "IX_OrdensDeServico_IDUsuario",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Aquecida",
                table: "Piscinas");

            migrationBuilder.DropColumn(
                name: "Coberta",
                table: "Piscinas");

            migrationBuilder.DropColumn(
                name: "Alcalinidade",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "Aprovador",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "CloroLivre",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "DurezaCalcica",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "FotosAntes",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "FotosDepois",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "HoraInicio",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "HoraTermino",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "IDUsuario",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "pH",
                table: "OrdensDeServico");

            migrationBuilder.RenameColumn(
                name: "FotosOcorrencias",
                table: "OrdensDeServico",
                newName: "FotosAntesDepois");

            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "Equipamentos",
                newName: "Tipo");
        }
    }
}
