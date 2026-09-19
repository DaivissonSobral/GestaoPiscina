using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;

if (args.Length < 2)
{
    Console.WriteLine("Uso: DataMigration <caminho-do-GestaoPiscina.db> <connection-string-sql-server>");
    return 1;
}

var sqlitePath = args[0];
var sqlServerConnectionString = args[1];

if (!File.Exists(sqlitePath))
{
    Console.WriteLine($"Arquivo não encontrado: {sqlitePath}");
    return 1;
}

// Backup online e consistente do SQLite (evita mexer no arquivo original, que pode
// estar em uso pelo processo da API ao vivo, e evita inconsistência de WAL não
// checkpointado se copiássemos os arquivos .db/-wal/-shm manualmente).
var tempCopyPath = Path.Combine(Path.GetTempPath(), $"GestaoPiscina_migracao_{Guid.NewGuid():N}.db");
Console.WriteLine($"Criando backup consistente do SQLite em: {tempCopyPath}");
using (var source = new SqliteConnection($"Data Source={sqlitePath};Mode=ReadOnly"))
using (var destination = new SqliteConnection($"Data Source={tempCopyPath}"))
{
    source.Open();
    destination.Open();
    source.BackupDatabase(destination);
}

var sqliteOptions = new DbContextOptionsBuilder<GestaoPiscinaContext>()
    .UseSqlite($"Data Source={tempCopyPath}")
    .Options;
var sqlServerOptions = new DbContextOptionsBuilder<GestaoPiscinaContext>()
    .UseSqlServer(sqlServerConnectionString)
    .Options;

using var sourceContext = new GestaoPiscinaContext(sqliteOptions);
using var targetContext = new GestaoPiscinaContext(sqlServerOptions);

await targetContext.Database.OpenConnectionAsync();

async Task CopyTable<T>(string tableName, Func<GestaoPiscinaContext, Task<List<T>>> loader, bool hasIdentity = true) where T : class
{
    var rows = await loader(sourceContext);
    Console.WriteLine($"{tableName}: {rows.Count} linha(s)");
    if (rows.Count == 0) return;

    if (hasIdentity)
        await targetContext.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] ON");

    targetContext.Set<T>().AddRange(rows);
    await targetContext.SaveChangesAsync();

    if (hasIdentity)
        await targetContext.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] OFF");

    // Evita reaproveitar as mesmas instâncias/tracking entre tabelas seguintes.
    foreach (var entry in targetContext.ChangeTracker.Entries().ToList())
        entry.State = EntityState.Detached;
}

// Ordem respeita as FKs (pais antes dos filhos).
await CopyTable<Perfil>("Perfis", c => c.Perfis.AsNoTracking().ToListAsync());
await CopyTable<Produto>("Produtos", c => c.Produtos.AsNoTracking().ToListAsync());
await CopyTable<Cliente>("Clientes", c => c.Clientes.AsNoTracking().ToListAsync());
await CopyTable<Gestor>("Gestores", c => c.Gestores.AsNoTracking().ToListAsync());
await CopyTable<ChecklistItem>("ChecklistItens", c => c.ChecklistItens.AsNoTracking().ToListAsync());
await CopyTable<Usuario>("Usuarios", c => c.Usuarios.AsNoTracking().ToListAsync());
await CopyTable<GestorCliente>("GestorClientes", c => c.GestorClientes.AsNoTracking().ToListAsync(), hasIdentity: false);
await CopyTable<Piscina>("Piscinas", c => c.Piscinas.AsNoTracking().ToListAsync());
await CopyTable<Equipamento>("Equipamentos", c => c.Equipamentos.AsNoTracking().ToListAsync());
await CopyTable<EstoqueCliente>("EstoqueClientes", c => c.EstoqueClientes.AsNoTracking().ToListAsync());
await CopyTable<RotaVisita>("RotaVisitas", c => c.RotaVisitas.AsNoTracking().ToListAsync());
await CopyTable<PushSubscriptionRegistro>("PushSubscriptionRegistros", c => c.PushSubscriptionRegistros.AsNoTracking().ToListAsync());
await CopyTable<OrdemDeServico>("OrdensDeServico", c => c.OrdensDeServico.AsNoTracking().ToListAsync());
await CopyTable<DosagemProduto>("DosagensProdutos", c => c.DosagensProdutos.AsNoTracking().ToListAsync());
await CopyTable<MovimentacaoEstoque>("MovimentacoesEstoque", c => c.MovimentacoesEstoque.AsNoTracking().ToListAsync());

await targetContext.Database.CloseConnectionAsync();
File.Delete(tempCopyPath);
Console.WriteLine("Migração concluída com sucesso.");
return 0;
