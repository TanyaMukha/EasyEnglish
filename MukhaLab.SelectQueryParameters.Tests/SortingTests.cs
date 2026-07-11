using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Tests;

public class SortingTests
{
    private static IQueryable<TestItem> Items() => TestData.SampleItems().AsQueryable();

    [Fact]
    public void ApplySorting_SingleKeyAscending_OrdersCorrectly()
    {
        var sort = new List<SortDescriptor> { new() { Field = "Title", Direction = SortDirection.Asc } };

        var result = Items().ApplySorting(sort).ToList();

        Assert.Equal(["Apple", "Banana", "Cherry", "Date"], result.Select(i => i.Title));
    }

    [Fact]
    public void ApplySorting_SingleKeyDescending_OrdersCorrectly()
    {
        var sort = new List<SortDescriptor> { new() { Field = "Quantity", Direction = SortDirection.Desc } };

        var result = Items().ApplySorting(sort).ToList();

        Assert.Equal([20, 10, 5, 0], result.Select(i => i.Quantity));
    }

    [Fact]
    public void ApplySorting_MultipleKeys_AppliesThenBy()
    {
        // IsActive desc (true first), then Title asc within each group.
        var sort = new List<SortDescriptor>
        {
            new() { Field = "IsActive", Direction = SortDirection.Desc },
            new() { Field = "Title", Direction = SortDirection.Asc }
        };

        var result = Items().ApplySorting(sort).ToList();

        Assert.Equal(["Apple", "Cherry", "Date", "Banana"], result.Select(i => i.Title));
    }

    [Fact]
    public void ApplySorting_BlankFieldDescriptor_IsSkipped()
    {
        var sort = new List<SortDescriptor>
        {
            new() { Field = "", Direction = SortDirection.Asc },
            new() { Field = "Title", Direction = SortDirection.Asc }
        };

        var result = Items().ApplySorting(sort).ToList();

        Assert.Equal(["Apple", "Banana", "Cherry", "Date"], result.Select(i => i.Title));
    }

    [Fact]
    public void ApplySorting_EmptyList_ReturnsUnmodifiedOrder()
    {
        var original = Items().ToList();

        var result = Items().ApplySorting([]).ToList();

        Assert.Equal(original.Select(i => i.Id), result.Select(i => i.Id));
    }
}
