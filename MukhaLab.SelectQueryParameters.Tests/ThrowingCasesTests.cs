using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Tests;

public class ThrowingCasesTests
{
    private static IQueryable<TestItem> Items() => TestData.SampleItems().AsQueryable();

    [Fact]
    public void GreaterThan_OnStringProperty_Throws()
    {
        var filter = new FilterParameter { Field = "Title", Operation = FilterOperation.GreaterThan, DataType = FilterDataType.String, Value = "B" };

        Assert.ThrowsAny<Exception>(() => Items().ApplyFilter(filter).ToList());
    }

    [Fact]
    public void Contains_OnNonStringProperty_Throws()
    {
        var filter = new FilterParameter { Field = "Quantity", Operation = FilterOperation.Contains, DataType = FilterDataType.Integer, Value = "5" };

        Assert.ThrowsAny<Exception>(() => Items().ApplyFilter(filter).ToList());
    }

    [Fact]
    public void ConvertFilterValue_NonNumericStringForIntegerDataType_ThrowsFormatException()
    {
        var filter = new FilterParameter { Field = "Quantity", Operation = FilterOperation.Equal, DataType = FilterDataType.Integer, Value = "not-a-number" };

        Assert.Throws<FormatException>(() => Items().ApplyFilter(filter).ToList());
    }

    [Fact]
    public void ConvertFilterValue_InvalidGuidString_Throws()
    {
        var filter = new FilterParameter { Field = "ExternalId", Operation = FilterOperation.Equal, DataType = FilterDataType.Guid, Value = "not-a-guid" };

        Assert.ThrowsAny<Exception>(() => Items().ApplyFilter(filter).ToList());
    }
}
