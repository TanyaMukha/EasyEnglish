using MukhaLab.SelectQueryParameters.Extensions;

namespace MukhaLab.SelectQueryParameters.Tests;

public class PagingTests
{
    private static IQueryable<TestItem> Items() => TestData.SampleItems().AsQueryable();

    [Fact]
    public void ApplyPaging_NormalPage_ReturnsCorrectSlice()
    {
        var result = Items().OrderBy(i => i.Id).ApplyPaging(pageNumber: 2, pageSize: 2).ToList();

        Assert.Equal([3, 4], result.Select(i => i.Id));
    }

    [Fact]
    public void ApplyPaging_PageNumberBelowOne_ClampsToFirstPage()
    {
        var result = Items().OrderBy(i => i.Id).ApplyPaging(pageNumber: 0, pageSize: 2).ToList();

        Assert.Equal([1, 2], result.Select(i => i.Id));
    }

    [Fact]
    public void ApplyPaging_PageSizeBelowOne_ClampsToTen()
    {
        // 4 sample items all fit within a clamped page size of 10.
        var result = Items().OrderBy(i => i.Id).ApplyPaging(pageNumber: 1, pageSize: 0).ToList();

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void ApplyPaging_PageBeyondData_ReturnsEmpty()
    {
        var result = Items().OrderBy(i => i.Id).ApplyPaging(pageNumber: 5, pageSize: 2).ToList();

        Assert.Empty(result);
    }
}
