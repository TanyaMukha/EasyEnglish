using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyPeasy.Data.Tests.Fixtures;

/// <summary>
/// Shared base for EasyPeasy.Data tests: opens one SQLite in-memory connection per test class
/// instance (xUnit creates a fresh instance per [Fact], giving full isolation), builds the real
/// <see cref="EasyPeasyDbContext"/> schema against it, and exposes ready-to-use repositories.
/// Uses real SQLite (not EF Core's <c>InMemory</c> provider) specifically to catch LINQ-to-SQL
/// translation issues that <c>InMemory</c> wouldn't reproduce.
/// </summary>
public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly IDbContextFactory<EasyPeasyDbContext> Factory;
    protected readonly IUserContext UserContext = new AnonymousUserContext();

    protected SqliteTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<EasyPeasyDbContext>().UseSqlite(_connection).Options;
        Factory = new SimpleDbContextFactory(options);

        using var ctx = Factory.CreateDbContext();
        ctx.Database.EnsureCreated();
    }

    /// <summary>Opens a fresh <see cref="EasyPeasyDbContext"/> against the shared in-memory database — for arranging test data.</summary>
    protected EasyPeasyDbContext CreateContext() => Factory.CreateDbContext();

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed class SimpleDbContextFactory(DbContextOptions<EasyPeasyDbContext> options) : IDbContextFactory<EasyPeasyDbContext>
    {
        public EasyPeasyDbContext CreateDbContext() => new(options);
    }
}
