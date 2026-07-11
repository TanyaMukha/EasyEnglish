using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Tests;

/// <summary>
/// Regression tests for the QueryParameters constructor's PageNumber defaulting logic: an
/// explicitly supplied pageNumber must never be silently overwritten, and PageNumber must only
/// default to 1 when paging was actually requested via rowCount.
/// </summary>
public class QueryParametersConstructorTests
{
    [Fact]
    public void ExplicitPageNumber_WithoutRowCount_IsPreserved()
    {
        var parameters = new QueryParameters(pageNumber: 5);

        Assert.Equal(5, parameters.PageNumber);
        Assert.Null(parameters.RowCount);
    }

    [Fact]
    public void NoArguments_PageNumberStaysNull()
    {
        var parameters = new QueryParameters();

        Assert.Null(parameters.PageNumber);
        Assert.Null(parameters.RowCount);
    }

    [Fact]
    public void RowCountOnly_PageNumberDefaultsToOne()
    {
        var parameters = new QueryParameters(rowCount: 20);

        Assert.Equal(1, parameters.PageNumber);
        Assert.Equal(20, parameters.RowCount);
    }

    [Fact]
    public void PageNumberAndRowCount_UsesExplicitPageNumber()
    {
        var parameters = new QueryParameters(pageNumber: 3, rowCount: 20);

        Assert.Equal(3, parameters.PageNumber);
        Assert.Equal(20, parameters.RowCount);
    }
}
