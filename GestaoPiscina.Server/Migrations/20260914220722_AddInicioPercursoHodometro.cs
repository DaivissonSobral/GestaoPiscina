using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddInicioPercursoHodometro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoHodometro",
                table: "OrdensDeServico",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InicioPercurso",
                table: "OrdensDeServico",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoHodometro",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "InicioPercurso",
                table: "OrdensDeServico");
        }
    }
}
