using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRecorrenciaPiscina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RecorrenciaDataFim",
                table: "Piscinas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecorrenciaDataInicio",
                table: "Piscinas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecorrenciaDiasSemana",
                table: "Piscinas",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecorrenciaFrequencia",
                table: "Piscinas",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Nenhuma");

            migrationBuilder.AddColumn<int>(
                name: "RecorrenciaIntervalo",
                table: "Piscinas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "RecorrenciaOcorrencias",
                table: "Piscinas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecorrenciaTermino",
                table: "Piscinas",
                type: "TEXT",
                maxLength: 15,
                nullable: false,
                defaultValue: "Nunca");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecorrenciaDataFim",
                table: "Piscinas");

            migrationBuilder.DropColumn(
                name: "RecorrenciaDataInicio",
                table: "Piscinas");

            migrationBuilder.DropColumn(
                name: "RecorrenciaDiasSemana",
                table: "Piscinas");

            migrationBuilder.DropColumn(
                name: "RecorrenciaFrequencia",
                table: "Piscinas");

            migrationBuilder.DropColumn(
                name: "RecorrenciaIntervalo",
                table: "Piscinas");

            migrationBuilder.DropColumn(
                name: "RecorrenciaOcorrencias",
                table: "Piscinas");

            migrationBuilder.DropColumn(
                name: "RecorrenciaTermino",
                table: "Piscinas");
        }
    }
}
