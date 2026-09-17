using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameVolumeLitrosToVolumeM3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VolumeLitros",
                table: "Piscinas",
                newName: "VolumeM3");

            // Piscinas já cadastradas guardavam o volume em litros; converte para m³
            // mantendo o volume real (ex.: 55000 litros -> 55 m³).
            migrationBuilder.Sql("UPDATE Piscinas SET VolumeM3 = CAST(VolumeM3 AS REAL) / 1000;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Piscinas SET VolumeM3 = CAST(VolumeM3 AS REAL) * 1000;");

            migrationBuilder.RenameColumn(
                name: "VolumeM3",
                table: "Piscinas",
                newName: "VolumeLitros");
        }
    }
}
