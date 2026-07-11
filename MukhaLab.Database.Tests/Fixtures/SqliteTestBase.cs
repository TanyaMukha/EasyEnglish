using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MukhaLab.Database;

namespace MukhaLab.Database.Tests.Fixtures;

/// <summary>
/// Shared base for MukhaLab.Database tests: opens one SQLite in-memory connection per test class
/// instance (xUnit creates a fresh instance per [Fact], giving full isolation) and exposes ready-to-use
/// repositories/services backed by it.
/// </summary>
public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly IDbContextFactory<TestDbContext> Factory;
    protected readonly IMapper Mapper;

    protected SqliteTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options;
        Factory = new SimpleDbContextFactory(options);

        using (var ctx = Factory.CreateDbContext())
        {
            ctx.Database.EnsureCreated();
        }

        Mapper = new MapperConfiguration(cfg => cfg.AddProfile<TestMappingProfile>(), NullLoggerFactory.Instance).CreateMapper();
    }

    protected TestRepository CreateRepository(IUserContext? userContext = null) => new(Factory, userContext);

    protected TestGuidRepository CreateGuidRepository(IUserContext? userContext = null) => new(Factory, userContext);

    protected TestService CreateService(IUserContext? userContext = null) =>
        new(CreateRepository(userContext), Mapper, NullLogger<BaseService<TestEntity, TestModel>>.Instance);

    public void Dispose()
    {
        _connection.Dispose();
    }

    private sealed class SimpleDbContextFactory(DbContextOptions<TestDbContext> options) : IDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext() => new(options);
    }
}
