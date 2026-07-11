using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Tests;

public class SqlTestChild
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public int SqlTestItemId { get; set; }
}

public class SqlTestItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public List<SqlTestChild> Children { get; set; } = new();
}

public class SqlTestDbContext(DbContextOptions<SqlTestDbContext> options) : DbContext(options)
{
    public DbSet<SqlTestItem> Items => Set<SqlTestItem>();
}

/// <summary>
/// Supplementary smoke tests confirming the expression trees this library builds are actually
/// translatable to SQL by a real relational provider (SQLite, matching the app's real provider),
/// not just compatible with LINQ-to-Objects. In particular this exercises the collection-path
/// filter as a correlated subquery, which the pure in-memory tests elsewhere in this project
/// can't verify.
/// </summary>
public class SqliteTranslationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SqlTestDbContext _context;

    public SqliteTranslationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SqlTestDbContext>().UseSqlite(_connection).Options;
        _context = new SqlTestDbContext(options);
        _context.Database.EnsureCreated();

        _context.Items.AddRange(
            new SqlTestItem
            {
                Title = "Apple",
                Quantity = 10,
                CreatedAt = new DateTime(2024, 1, 10),
                ReviewedAt = null,
                Children = [new SqlTestChild { Title = "John" }]
            },
            new SqlTestItem
            {
                Title = "Banana",
                Quantity = 5,
                CreatedAt = new DateTime(2024, 2, 15),
                ReviewedAt = new DateTime(2024, 3, 1),
                Children = [new SqlTestChild { Title = "Bob" }]
            });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Equal_String_TranslatesAndExecutes()
    {
        var filter = new FilterParameter { Field = "Title", Operation = FilterOperation.Equal, DataType = FilterDataType.String, Value = "Apple" };

        var result = await _context.Items.ApplyFilter(filter).ToListAsync();

        var item = Assert.Single(result);
        Assert.Equal("Apple", item.Title);
    }

    [Fact]
    public async Task Contains_String_TranslatesAndExecutes()
    {
        var filter = new FilterParameter { Field = "Title", Operation = FilterOperation.Contains, DataType = FilterDataType.String, Value = "an" };

        var result = await _context.Items.ApplyFilter(filter).ToListAsync();

        var item = Assert.Single(result);
        Assert.Equal("Banana", item.Title);
    }

    [Fact]
    public async Task GreaterThan_Integer_TranslatesAndExecutes()
    {
        var filter = new FilterParameter { Field = "Quantity", Operation = FilterOperation.GreaterThan, DataType = FilterDataType.Integer, Value = "7" };

        var result = await _context.Items.ApplyFilter(filter).ToListAsync();

        var item = Assert.Single(result);
        Assert.Equal("Apple", item.Title);
    }

    [Fact]
    public async Task IsNull_NullableDateTime_TranslatesAndExecutes()
    {
        var filter = new FilterParameter { Field = "ReviewedAt", Operation = FilterOperation.IsNull, DataType = FilterDataType.DateTime };

        var result = await _context.Items.ApplyFilter(filter).ToListAsync();

        var item = Assert.Single(result);
        Assert.Equal("Apple", item.Title);
    }

    [Fact]
    public async Task CollectionPath_Contains_TranslatesAsCorrelatedSubqueryAndExecutes()
    {
        var filter = new FilterParameter { Field = "Children[Title]", Operation = FilterOperation.Contains, DataType = FilterDataType.String, Value = "Jo" };

        var result = await _context.Items.ApplyFilter(filter).ToListAsync();

        var item = Assert.Single(result);
        Assert.Equal("Apple", item.Title);
    }

    [Fact]
    public async Task ApplyQueryParameters_FullComposition_TranslatesAndExecutes()
    {
        var parameters = new QueryParameters
        {
            PageNumber = 1,
            RowCount = 10,
            Filters = [new FilterParameter { Field = "Quantity", Operation = FilterOperation.GreaterThan, DataType = FilterDataType.Integer, Value = "0" }],
            Sort = [new SortDescriptor { Field = "Title", Direction = SortDirection.Asc }]
        };

        var result = await _context.Items.ApplyQueryParameters(parameters).ToListAsync();

        Assert.Equal(["Apple", "Banana"], result.Select(i => i.Title));
    }
}
