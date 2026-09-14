using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizacaoInicioOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LatitudeInicio",
                table: "OrdensDeServico",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LongitudeInicio",
                table: "OrdensDeServico",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatitudeInicio",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "LongitudeInicio",
                table: "OrdensDeServico");
        }
    }
}
