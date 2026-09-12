using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;

namespace GestaoPiscina.Server.Tests.TestSupport
{
    // Cada teste recebe seu próprio banco SQLite em memória, isolado dos demais.
    // Diferente do provider "InMemory" do EF Core, o SQLite real suporta
    // transações, FKs e GroupBy/Sum traduzidos para SQL — mais fiel ao que
    // roda em produção (GestaoPiscinaContext também usa UseSqlite).
    public sealed class SqliteInMemoryContext : IDisposable
    {
        private readonly SqliteConnection _connection;

        public GestaoPiscinaContext Context { get; }

        public SqliteInMemoryContext()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<GestaoPiscinaContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new GestaoPiscinaContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
