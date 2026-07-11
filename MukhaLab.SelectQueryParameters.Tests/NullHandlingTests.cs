using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Tests;

/// <summary>
/// Regression tests for IsNull/IsNotNull on non-nullable value type properties: these must resolve
/// to a constant true/false instead of throwing when building the expression tree.
/// </summary>
public class NullHandlingTests
{
    private static IQueryable<TestItem> Items() => TestData.SampleItems().AsQueryable();

    [Fact]
    public void IsNull_OnNonNullableValueType_DoesNotThrow_ReturnsEmpty()
    {
        // CreatedAt is a non-nullable DateTime, which can never be null.
        var filter = new FilterParameter { Field = "CreatedAt", Operation = FilterOperation.IsNull, DataType = FilterDataType.DateTime };

        var exception = Record.Exception(() => Items().ApplyFilter(filter).ToList());

        Assert.Null(exception);
    }

    [Fact]
    public void IsNull_OnNonNullableValueType_ResolvesToConstantFalse()
    {
        var filter = new FilterParameter { Field = "CreatedAt", Operation = FilterOperation.IsNull, DataType = FilterDataType.DateTime };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void IsNotNull_OnNonNullableValueType_DoesNotThrow_ReturnsAll()
    {
        var filter = new FilterParameter { Field = "CreatedAt", Operation = FilterOperation.IsNotNull, DataType = FilterDataType.DateTime };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void IsNull_OnNullableValueType_StillFiltersByActualNullness()
    {
        // ArchivedId is a nullable Guid?; only item 2 (Banana) has a non-null value.
        var filter = new FilterParameter { Field = "ArchivedId", Operation = FilterOperation.IsNull, DataType = FilterDataType.Guid };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal([1, 3, 4], result.Select(i => i.Id).OrderBy(x => x));
    }

    [Fact]
    public void IsNotNull_OnNullableValueType_StillFiltersByActualNullness()
    {
        var filter = new FilterParameter { Field = "ArchivedId", Operation = FilterOperation.IsNotNull, DataType = FilterDataType.Guid };

        var result = Items().ApplyFilter(filter).ToList();

        var item = Assert.Single(result);
        Assert.Equal(2, item.Id);
    }

    [Fact]
    public void IsNull_OnReferenceType_FiltersByNullness()
    {
        // Author is a nullable reference-typed navigation, null for items 1-3.
        var filter = new FilterParameter { Field = "Author", Operation = FilterOperation.IsNull, DataType = FilterDataType.String };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Equal([1, 2, 3], result.Select(i => i.Id).OrderBy(x => x));
    }
}
