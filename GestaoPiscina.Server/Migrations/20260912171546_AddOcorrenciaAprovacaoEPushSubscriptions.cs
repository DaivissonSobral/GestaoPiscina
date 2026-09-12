using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddOcorrenciaAprovacaoEPushSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAprovacaoOcorrencia",
                table: "OrdensDeServico",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OcorrenciaAprovada",
                table: "OrdensDeServico",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PushSubscriptionRegistros",
                columns: table => new
                {
                    IDPushSubscriptionRegistro = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDUsuario = table.Column<int>(type: "INTEGER", nullable: false),
                    Endpoint = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    P256dh = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Auth = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptionRegistros_Endpoint",
                table: "PushSubscriptionRegistros",
                column: "Endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptionRegistros_IDUsuario",
                table: "PushSubscriptionRegistros",
                column: "IDUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PushSubscriptionRegistros");

            migrationBuilder.DropColumn(
                name: "DataAprovacaoOcorrencia",
                table: "OrdensDeServico");

            migrationBuilder.DropColumn(
                name: "OcorrenciaAprovada",
                table: "OrdensDeServico");
        }
    }
}
