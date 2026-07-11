using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Tests;

public class FilterOperationTests
{
    private static IQueryable<TestItem> Items() => TestData.SampleItems().AsQueryable();

    [Fact]
    public void Equal_String_ReturnsExactMatch()
    {
        var filter = new FilterParameter { Field = "Title", Operation = FilterOperation.Equal, DataType = FilterDataType.String, Value = "Apple" };

        var result = Items().ApplyFilter(filter).ToList();

        var item = Assert.Single(result);
        Assert.Equal(1, item.Id);
    }

    [Fact]
    public void NotEqual_String_ExcludesMatch()
    {
        var filter = new FilterParameter { Field = "Title", Operation = FilterOperation.NotEqual, DataType = FilterDataType.String, Value = "Apple" };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, i => i.Title == "Apple");
    }

    [Fact]
    public void GreaterThan_Integer_ReturnsItemsAboveThreshold()
    {
        var filter = new FilterParameter { Field = "Quantity", Operation = FilterOperation.GreaterThan, DataType = FilterDataType.Integer, Value = "5" };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal([1, 3], result.Select(i => i.Id).OrderBy(x => x));
    }

    [Fact]
    public void GreaterThanOrEqual_Integer_IncludesBoundary()
    {
        var filter = new FilterParameter { Field = "Quantity", Operation = FilterOperation.GreaterThanOrEqual, DataType = FilterDataType.Integer, Value = "5" };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal([1, 2, 3], result.Select(i => i.Id).OrderBy(x => x));
    }

    [Fact]
    public void LessThan_Decimal_ReturnsItemsBelowThreshold()
    {
        var filter = new FilterParameter { Field = "Price", Operation = FilterOperation.LessThan, DataType = FilterDataType.Decimal, Value = "2.00" };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal([1, 2], result.Select(i => i.Id).OrderBy(x => x));
    }

    [Fact]
    public void LessThanOrEqual_Decimal_IncludesBoundary()
    {
        var filter = new FilterParameter { Field = "Price", Operation = FilterOperation.LessThanOrEqual, DataType = FilterDataType.Decimal, Value = "1.50" };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal([1, 2], result.Select(i => i.Id).OrderBy(x => x));
    }

    [Fact]
    public void Contains_String_MatchesSubstring()
    {
        var filter = new FilterParameter { Field = "Title", Operation = FilterOperation.Contains, DataType = FilterDataType.String, Value = "an" };

        var result = Items().ApplyFilter(filter).ToList();

        var item = Assert.Single(result);
        Assert.Equal("Banana", item.Title);
    }

    [Fact]
    public void StartsWith_String_MatchesPrefix()
    {
        var filter = new FilterParameter { Field = "Title", Operation = FilterOperation.StartsWith, DataType = FilterDataType.String, Value = "Ch" };

        var result = Items().ApplyFilter(filter).ToList();

        var item = Assert.Single(result);
        Assert.Equal("Cherry", item.Title);
    }

    [Fact]
    public void EndsWith_String_MatchesSuffix()
    {
        var filter = new FilterParameter { Field = "Title", Operation = FilterOperation.EndsWith, DataType = FilterDataType.String, Value = "e" };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal(["Apple", "Date"], result.Select(i => i.Title).OrderBy(x => x));
    }

    [Fact]
    public void Between_DateTime_ReturnsItemsWithinRange()
    {
        var filter = new FilterParameter
        {
            Field = "CreatedAt",
            Operation = FilterOperation.Between,
            DataType = FilterDataType.DateTime,
            From = "2024-01-01",
            To = "2024-01-31"
        };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal([1, 3], result.Select(i => i.Id).OrderBy(x => x));
    }

    [Fact]
    public void Equal_Boolean_ReturnsMatchingItems()
    {
        var filter = new FilterParameter { Field = "IsActive", Operation = FilterOperation.Equal, DataType = FilterDataType.Boolean, Value = "true" };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal([1, 3, 4], result.Select(i => i.Id).OrderBy(x => x));
    }

    [Fact]
    public void Equal_Guid_ReturnsMatchingItem()
    {
        var filter = new FilterParameter { Field = "ExternalId", Operation = FilterOperation.Equal, DataType = FilterDataType.Guid, Value = TestData.BananaExternalId.ToString() };

        var result = Items().ApplyFilter(filter).ToList();

        var item = Assert.Single(result);
        Assert.Equal(2, item.Id);
    }

    [Fact]
    public void Date_DataType_TruncatesTimeComponent()
    {
        // CreatedAt for item 1 is stored as midnight (2024-01-10T00:00:00). Supplying a query
        // value with a non-midnight time and FilterDataType.Date must still match, proving the
        // query value itself gets truncated to its date component before comparison.
        var filter = new FilterParameter
        {
            Field = "CreatedAt",
            Operation = FilterOperation.Equal,
            DataType = FilterDataType.Date,
            Value = "2024-01-10T15:45:30"
        };

        var result = Items().ApplyFilter(filter).ToList();

        var item = Assert.Single(result);
        Assert.Equal(1, item.Id);
    }
}
