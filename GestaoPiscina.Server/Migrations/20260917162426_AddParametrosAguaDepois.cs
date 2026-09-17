using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddParametrosAguaDepois : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AlcalinidadeDepois",
                table: "OrdensDeServico",
                type: "TEXT",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CloroLivreDepois",
                table: "OrdensDeServico",
                type: "TEXT",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DurezaCalcicaDepois",
                table: "OrdensDeServico",
                type: "TEXT",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "pHDepois",
                table: "OrdensDeServico",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlcalinidadeDepois",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "CloroLivreDepois",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "DurezaCalcicaDepois",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "pHDepois",
                table: "OrdensDeServico");
        }
    }
}
