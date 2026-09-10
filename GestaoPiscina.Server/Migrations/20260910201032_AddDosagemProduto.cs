using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDosagemProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DosagensProdutos",
                columns: table => new
                {
                    IDDosagem = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDOS = table.Column<int>(type: "INTEGER", nullable: false),
                    IDProduto = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DosagensProdutos", x => x.IDDosagem);
                    table.ForeignKey(
                        name: "FK_DosagensProdutos_OrdensDeServico_IDOS",
                        column: x => x.IDOS,
                        principalTable: "OrdensDeServico",
                        principalColumn: "IDOS",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DosagensProdutos_Produtos_IDProduto",
                        column: x => x.IDProduto,
                        principalTable: "Produtos",
                        principalColumn: "IDProduto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DosagensProdutos_IDOS",
                table: "DosagensProdutos",
                column: "IDOS");

            migrationBuilder.CreateIndex(
                name: "IX_DosagensProdutos_IDProduto",
                table: "DosagensProdutos",
                column: "IDProduto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DosagensProdutos");
        }
    }
}
