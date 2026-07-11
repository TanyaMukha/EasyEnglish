using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Tests.Fixtures;

/// <summary>
/// Shared base for EasyEnglish.Data tests: opens one SQLite in-memory connection per test class
/// instance (xUnit creates a fresh instance per [Fact], giving full isolation), builds the real
/// <see cref="EasyEnglishDbContext"/> schema against it, and exposes ready-to-use repositories.
/// Uses real SQLite (not EF Core's <c>InMemory</c> provider) specifically to catch LINQ-to-SQL
/// translation issues that <c>InMemory</c> wouldn't reproduce.
/// </summary>
public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly IDbContextFactory<EasyEnglishDbContext> Factory;
    protected readonly IUserContext UserContext = new AnonymousUserContext();

    protected SqliteTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<EasyEnglishDbContext>().UseSqlite(_connection).Options;
        Factory = new SimpleDbContextFactory(options);

        using var ctx = Factory.CreateDbContext();
        ctx.Database.EnsureCreated();
    }

    /// <summary>Opens a fresh <see cref="EasyEnglishDbContext"/> against the shared in-memory database — for arranging test data.</summary>
    protected EasyEnglishDbContext CreateContext() => Factory.CreateDbContext();

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed class SimpleDbContextFactory(DbContextOptions<EasyEnglishDbContext> options) : IDbContextFactory<EasyEnglishDbContext>
    {
        public EasyEnglishDbContext CreateDbContext() => new(options);
    }
}
