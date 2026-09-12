using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPiscina.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistItens",
                columns: table => new
                {
                    IDChecklistItem = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Chave = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Texto = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    TiposClienteObrigatorio = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TiposPiscinaObrigatorio = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItens", x => x.IDChecklistItem);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItens_Chave",
                table: "ChecklistItens",
                column: "Chave",
                unique: true);

            // Migra os 5 itens fixos que existiam hardcoded em ChecklistOS.cs (Client),
            // preservando as mesmas chaves já gravadas no campo OrdemDeServico.ChecklistItens
            // de OS existentes, e marcando obrigatório para todos os tipos de cliente/piscina
            // para não mudar o comportamento de nenhuma OS já em andamento.
            const string todosOsTiposCliente = "Uso Coletivo,Residencial Privativa";
            const string todosOsTiposPiscina = "adulto,infantil,espelho";
            migrationBuilder.InsertData(
                table: "ChecklistItens",
                columns: new[] { "Chave", "Texto", "Ordem", "Ativo", "TiposClienteObrigatorio", "TiposPiscinaObrigatorio" },
                values: new object[,]
                {
                    { "cesto", "Realizamos a limpeza do cesto pré-filtro.", 1, true, todosOsTiposCliente, todosOsTiposPiscina },
                    { "aspiracao", "Foi realizada a aspiração filtrando, isso gera economia de água para o estabelecimento.", 2, true, todosOsTiposCliente, todosOsTiposPiscina },
                    { "bordas", "Realizamos a limpeza das bordas.", 3, true, todosOsTiposCliente, todosOsTiposPiscina },
                    { "material", "Foi realizada a retirada de material suspenso.", 4, true, todosOsTiposCliente, todosOsTiposPiscina },
                    { "retrolavagem", "Realizamos a retrolavagem do filtro.", 5, true, todosOsTiposCliente, todosOsTiposPiscina }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistItens");
        }

        // Down() acima já dropa a tabela inteira (o seed seria removido junto);
        // sem necessidade de reverter as linhas individualmente.
    }
}
