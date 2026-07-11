using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Tests;

public class ApplyQueryParametersTests
{
    private static IQueryable<TestItem> Items() => TestData.SampleItems().AsQueryable();

    [Fact]
    public void ApplyQueryParameters_NullParameters_ReturnsUnmodifiedQuery()
    {
        var result = Items().ApplyQueryParameters(null!).ToList();

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void ApplyQueryParameters_FiltersOnly_NoSortingOrPaging()
    {
        var parameters = new QueryParameters
        {
            Filters = [new FilterParameter { Field = "IsActive", Operation = FilterOperation.Equal, DataType = FilterDataType.Boolean, Value = "true" }]
        };

        var result = Items().ApplyQueryParameters(parameters).ToList();

        Assert.Equal(3, result.Count);
        Assert.All(result, i => Assert.True(i.IsActive));
    }

    [Fact]
    public void ApplyQueryParameters_FullComposition_FiltersSortsAndPagesTogether()
    {
        var parameters = new QueryParameters
        {
            PageNumber = 1,
            RowCount = 1,
            Filters = [new FilterParameter { Field = "IsActive", Operation = FilterOperation.Equal, DataType = FilterDataType.Boolean, Value = "true" }],
            Sort = [new SortDescriptor { Field = "Title", Direction = SortDirection.Asc }]
        };

        var result = Items().ApplyQueryParameters(parameters).ToList();

        // Active items sorted by Title are Apple, Cherry, Date; page 1 of size 1 is just Apple.
        var item = Assert.Single(result);
        Assert.Equal("Apple", item.Title);
    }

    [Fact]
    public void ApplyQueryParameters_PagingRequiresBothPageNumberAndRowCount()
    {
        var parameters = new QueryParameters
        {
            PageNumber = 1,
            RowCount = null,
            Sort = [new SortDescriptor { Field = "Id", Direction = SortDirection.Asc }]
        };

        var result = Items().ApplyQueryParameters(parameters).ToList();

        // RowCount is missing, so paging must not apply even though PageNumber is set.
        Assert.Equal(4, result.Count);
    }
}
