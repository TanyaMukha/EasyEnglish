namespace MukhaLab.Database;

/// <summary>
/// Result of <see cref="IBaseRepository{T}.GetPaginationInfoAsync"/>: the total number of rows
/// matching a query's filters (ignoring paging) and the resulting page count.
/// </summary>
public class PaginationInfo
{
    /// <summary>Total number of rows matching the query's filters, ignoring <see cref="MukhaLab.SelectQueryParameters.Models.QueryParameters.PageNumber"/>/<see cref="MukhaLab.SelectQueryParameters.Models.QueryParameters.RowCount"/>.</summary>
    public int TotalCount { get; set; }

    /// <summary>Number of pages of size <see cref="MukhaLab.SelectQueryParameters.Models.QueryParameters.RowCount"/> needed to cover <see cref="TotalCount"/> rows.</summary>
    public int TotalPages { get; set; }
}
