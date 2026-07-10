namespace MukhaLab.SelectQueryParameters.Models;

/// <summary>
/// Aggregates filtering, sorting, and paging instructions for a single query. Pass an instance to
/// <c>QueryHelperExtensions.ApplyQueryParameters</c> to translate it into <see cref="System.Linq.Expressions.Expression"/>
/// trees applied to an <see cref="IQueryable{T}"/>.
/// </summary>
/// <example>
/// <code>
/// var queryParams = new QueryParameters
/// {
///     PageNumber = 1,
///     RowCount = 10,
///     Filters = new List&lt;FilterParameter&gt;
///     {
///         new FilterParameter
///         {
///             Field = "Title",
///             Operation = FilterOperation.Contains,
///             DataType = FilterDataType.String,
///             Value = "John"
///         }
///     },
///     Sort = new List&lt;SortDescriptor&gt;
///     {
///         new SortDescriptor { Field = "Title", Direction = SortDirection.Asc }
///     }
/// };
///
/// var page = dbContext.Tasks.ApplyQueryParameters(queryParams).ToList();
/// </code>
/// </example>
public class QueryParameters
{
    /// <summary>
    /// One-based page number. Paging is only applied by <c>ApplyQueryParameters</c> when both
    /// <see cref="PageNumber"/> and <see cref="RowCount"/> have a value. When constructed with a
    /// <see cref="RowCount"/> but no explicit page number, defaults to 1 (see the constructor
    /// remarks below).
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>Page size. Paging is only applied when this and <see cref="PageNumber"/> both have a value.</summary>
    public int? RowCount { get; init; }

    /// <summary>Sort keys, applied in list order (first key is the primary sort). Null or empty means "no sorting".</summary>
    public List<SortDescriptor>? Sort { get; init; }

    /// <summary>Filters to apply before sorting and paging. Null or empty means "no filtering".</summary>
    public List<FilterParameter>? Filters { get; init; }

    /// <param name="pageNumber">One-based page number. Used as given when provided; has no effect unless <paramref name="rowCount"/> is also set.</param>
    /// <param name="rowCount">Page size. When provided without <paramref name="pageNumber"/>, the page number defaults to 1.</param>
    /// <param name="sort">Sort keys, applied in list order.</param>
    /// <param name="filters">Filters to apply before sorting and paging.</param>
    /// <remarks>
    /// <see cref="PageNumber"/> is set to <paramref name="pageNumber"/> when it is provided; if it is
    /// omitted, it defaults to 1 only when <paramref name="rowCount"/> is set (i.e. paging was
    /// actually requested), and otherwise stays <c>null</c>. An explicitly supplied
    /// <paramref name="pageNumber"/> is never overwritten, even if <paramref name="rowCount"/> is
    /// omitted.
    /// </remarks>
    public QueryParameters(
        int? pageNumber = null,
        int? rowCount = null,
        List<SortDescriptor>? sort = null,
        List<FilterParameter>? filters = null)
    {
        PageNumber = pageNumber ?? (rowCount.HasValue ? 1 : null);
        RowCount = rowCount;
        Sort = sort;
        Filters = filters;
    }
}
