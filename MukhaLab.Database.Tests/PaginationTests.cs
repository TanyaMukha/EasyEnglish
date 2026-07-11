using MukhaLab.SelectQueryParameters.Models;
using MukhaLab.Database.Tests.Fixtures;

namespace MukhaLab.Database.Tests;

public class PaginationTests : SqliteTestBase
{
    private async Task SeedAsync(int count)
    {
        var repository = CreateRepository();
        for (var i = 1; i <= count; i++)
            await repository.AddAsync(new TestEntity { Title = $"Item{i:D2}", Quantity = i });
    }

    [Fact]
    public async Task GetPaginationInfoAsync_MatchesFilteredCountAcrossPages()
    {
        await SeedAsync(25);
        var repository = CreateRepository();

        var parameters = new QueryParameters
        {
            PageNumber = 1,
            RowCount = 10,
            Filters = [new FilterParameter { Field = "Quantity", Operation = FilterOperation.GreaterThan, DataType = FilterDataType.Integer, Value = "5" }]
        };

        var pagination = await repository.GetPaginationInfoAsync(parameters);

        // Quantities 6..25 match (20 rows), page size 10 -> 2 pages.
        Assert.Equal(20, pagination.TotalCount);
        Assert.Equal(2, pagination.TotalPages);
    }

    [Fact]
    public async Task GetAsync_WithQueryParameters_ReturnsRequestedPage()
    {
        await SeedAsync(25);
        var repository = CreateRepository();

        var parameters = new QueryParameters
        {
            PageNumber = 2,
            RowCount = 10,
            Sort = [new SortDescriptor { Field = "Quantity", Direction = SortDirection.Asc }]
        };

        var result = (await repository.GetAsync(parameters)).ToList();

        Assert.Equal(10, result.Count);
        Assert.Equal(11, result.First().Quantity);
        Assert.Equal(20, result.Last().Quantity);
    }

    [Fact]
    public async Task CountAsync_DiffersFromFilteredPaginationCount()
    {
        await SeedAsync(25);
        var repository = CreateRepository();

        var unfilteredCount = await repository.CountAsync();

        var parameters = new QueryParameters
        {
            Filters = [new FilterParameter { Field = "Quantity", Operation = FilterOperation.GreaterThan, DataType = FilterDataType.Integer, Value = "20" }]
        };
        var pagination = await repository.GetPaginationInfoAsync(parameters);

        Assert.Equal(25, unfilteredCount);
        Assert.Equal(5, pagination.TotalCount);
    }
}
