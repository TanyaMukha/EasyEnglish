namespace MukhaLib.SelectQueryParameters.Models;

/// <summary>
/// Represents the parameters used in a query for retrieving results with pagination, sorting, and filtering.
/// </summary>
public class QueryParameters
{
    /// <summary>
    /// The page number for pagination. If not provided, it defaults to the first page.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// The number of rows to be returned in the result set. If not specified, a default value will be used.
    /// </summary>
    public int? RowCount { get; set; }

    /// <summary>
    /// A dictionary representing the sorting criteria. 
    /// The key is the field name, and the value is the sorting direction ("ASC" for ascending or "DESC" for descending).
    /// </summary>
    public Dictionary<string, string?>? Sort { get; set; }

    /// <summary>
    /// A list of filter parameters to be applied to the query.
    /// Each filter condition will be used to narrow down the results based on specified field values.
    /// </summary>
    public List<FilterParameter>? Filters { get; set; }
}
