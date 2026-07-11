using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Tests;

/// <summary>
/// Regression tests for the collection-path filter fix: "Field[Property]" must compare the
/// collection item's property against the filter's actual value, not just check whether any item
/// has a non-null property (the old, broken behavior).
/// </summary>
public class CollectionFilterTests
{
    private static IQueryable<TestItem> Items() => TestData.SampleItems().AsQueryable();

    [Fact]
    public void Contains_OnCollectionPath_MatchesByValue()
    {
        var filter = new FilterParameter { Field = "Children[Title]", Operation = FilterOperation.Contains, DataType = FilterDataType.String, Value = "Jo" };

        var result = Items().ApplyFilter(filter).ToList();

        // Only item 1 (Apple) has a child whose Title contains "Jo" (John).
        var item = Assert.Single(result);
        Assert.Equal(1, item.Id);
    }

    [Fact]
    public void Contains_OnCollectionPath_WithNoMatchingValue_ReturnsEmpty()
    {
        // Regression: before the fix, a non-Equal operation on a collection path ignored the
        // filter value entirely and returned every item with at least one non-null-Title child
        // (items 1, 2, 4 here). The fixed behavior must return nothing, since no child's Title
        // actually contains "ZZZ".
        var filter = new FilterParameter { Field = "Children[Title]", Operation = FilterOperation.Contains, DataType = FilterDataType.String, Value = "ZZZ" };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Equal_OnCollectionPath_MatchesExactValue()
    {
        var filter = new FilterParameter { Field = "Children[Title]", Operation = FilterOperation.Equal, DataType = FilterDataType.String, Value = "Bob" };

        var result = Items().ApplyFilter(filter).ToList();

        var item = Assert.Single(result);
        Assert.Equal(2, item.Id);
    }

    [Fact]
    public void Equal_OnCollectionPath_WithNoMatch_ReturnsEmpty()
    {
        var filter = new FilterParameter { Field = "Children[Title]", Operation = FilterOperation.Equal, DataType = FilterDataType.String, Value = "NoSuchName" };

        var result = Items().ApplyFilter(filter).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void CollectionPath_WithNestedDotPathInsideBrackets_ResolvesNestedProperty()
    {
        var filter = new FilterParameter { Field = "Children[Author.Name]", Operation = FilterOperation.Equal, DataType = FilterDataType.String, Value = "Alice" };

        var result = Items().ApplyFilter(filter).ToList();

        // Only item 1's (Apple) single child has an Author whose Name is "Alice".
        var item = Assert.Single(result);
        Assert.Equal(1, item.Id);
    }

    [Fact]
    public void CollectionPath_PrecededByDotPrefix_ResolvesNestedCollection()
    {
        var filter = new FilterParameter { Field = "Parent.Children[Title]", Operation = FilterOperation.Equal, DataType = FilterDataType.String, Value = "Carol" };

        var result = Items().ApplyFilter(filter).ToList();

        // Only item 4 (Date) has a Parent whose Children collection contains "Carol".
        var item = Assert.Single(result);
        Assert.Equal(4, item.Id);
    }

    [Fact]
    public void CollectionPath_OnEmptyCollection_ReturnsNoMatch()
    {
        var filter = new FilterParameter { Field = "Children[Title]", Operation = FilterOperation.Contains, DataType = FilterDataType.String, Value = "any" };

        var result = Items().ApplyFilter(filter).ToList();

        // Item 3 (Cherry) has an empty Children collection and must never match.
        Assert.DoesNotContain(result, i => i.Id == 3);
    }
}
