using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimentacaoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    IDMovimentacao = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDCliente = table.Column<int>(type: "INTEGER", nullable: false),
                    IDProduto = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    QuantidadeContada = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IDDosagem = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.IDMovimentacao);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Clientes_IDCliente",
                        column: x => x.IDCliente,
                        principalTable: "Clientes",
                        principalColumn: "IDCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_DosagensProdutos_IDDosagem",
                        column: x => x.IDDosagem,
                        principalTable: "DosagensProdutos",
                        principalColumn: "IDDosagem",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Produtos_IDProduto",
                        column: x => x.IDProduto,
                        principalTable: "Produtos",
                        principalColumn: "IDProduto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_IDCliente_IDProduto",
                table: "MovimentacoesEstoque",
                columns: new[] { "IDCliente", "IDProduto" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_IDDosagem",
                table: "MovimentacoesEstoque",
                column: "IDDosagem");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_IDProduto",
                table: "MovimentacoesEstoque",
                column: "IDProduto");

            // Preserva o saldo já existente de cada item de estoque como um lançamento
            // de abertura, antes de remover a coluna QuantidadeAtual — sem isso, todo
            // saldo cadastrado até aqui seria perdido (viraria zero) na migração.
            migrationBuilder.Sql(@"
                INSERT INTO MovimentacoesEstoque (IDCliente, IDProduto, Tipo, Quantidade, Data, Observacao)
                SELECT IDCliente, IDProduto, 'Ajuste', QuantidadeAtual, datetime('now'), 'Saldo inicial migrado do cadastro anterior'
                FROM EstoqueClientes
                WHERE QuantidadeAtual <> 0;
            ");

            migrationBuilder.DropColumn(
                name: "QuantidadeAtual",
                table: "EstoqueClientes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.AddColumn<decimal>(
                name: "QuantidadeAtual",
                table: "EstoqueClientes",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
