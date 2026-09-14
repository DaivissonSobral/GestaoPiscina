using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOcorrenciaReprovacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataReprovacaoOcorrencia",
                table: "OrdensDeServico",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OcorrenciaReprovada",
                table: "OrdensDeServico",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataReprovacaoOcorrencia",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "OcorrenciaReprovada",
                table: "OrdensDeServico");
        }
    }
}
