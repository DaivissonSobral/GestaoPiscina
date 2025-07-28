using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddEstoqueClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstoquesCliente_Clientes_IDCliente",
                table: "EstoquesCliente");

            migrationBuilder.DropForeignKey(
                name: "FK_EstoquesCliente_Produtos_IDProduto",
                table: "EstoquesCliente");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstoquesCliente",
                table: "EstoquesCliente");

            migrationBuilder.RenameTable(
                name: "EstoquesCliente",
                newName: "EstoqueClientes");

            migrationBuilder.RenameIndex(
                name: "IX_EstoquesCliente_IDProduto",
                table: "EstoqueClientes",
                newName: "IX_EstoqueClientes_IDProduto");

            migrationBuilder.RenameIndex(
                name: "IX_EstoquesCliente_IDCliente",
                table: "EstoqueClientes",
                newName: "IX_EstoqueClientes_IDCliente");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstoqueClientes",
                table: "EstoqueClientes",
                column: "IDEstoque");

            migrationBuilder.AddForeignKey(
                name: "FK_EstoqueClientes_Clientes_IDCliente",
                table: "EstoqueClientes",
                column: "IDCliente",
                principalTable: "Clientes",
                principalColumn: "IDCliente",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EstoqueClientes_Produtos_IDProduto",
                table: "EstoqueClientes",
                column: "IDProduto",
                principalTable: "Produtos",
                principalColumn: "IDProduto",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstoqueClientes_Clientes_IDCliente",
                table: "EstoqueClientes");

            migrationBuilder.DropForeignKey(
                name: "FK_EstoqueClientes_Produtos_IDProduto",
                table: "EstoqueClientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstoqueClientes",
                table: "EstoqueClientes");

            migrationBuilder.RenameTable(
                name: "EstoqueClientes",
                newName: "EstoquesCliente");

            migrationBuilder.RenameIndex(
                name: "IX_EstoqueClientes_IDProduto",
                table: "EstoquesCliente",
                newName: "IX_EstoquesCliente_IDProduto");

            migrationBuilder.RenameIndex(
                name: "IX_EstoqueClientes_IDCliente",
                table: "EstoquesCliente",
                newName: "IX_EstoquesCliente_IDCliente");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstoquesCliente",
                table: "EstoquesCliente",
                column: "IDEstoque");

            migrationBuilder.AddForeignKey(
                name: "FK_EstoquesCliente_Clientes_IDCliente",
                table: "EstoquesCliente",
                column: "IDCliente",
                principalTable: "Clientes",
                principalColumn: "IDCliente",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EstoquesCliente_Produtos_IDProduto",
                table: "EstoquesCliente",
                column: "IDProduto",
                principalTable: "Produtos",
                principalColumn: "IDProduto",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
