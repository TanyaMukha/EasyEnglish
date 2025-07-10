namespace MukhaLib.SelectQueryParameters.Models;

/// <summary>
/// Represents the parameters for a GET request to retrieve query results.
/// </summary>
public class GetRequestParameters
{
    /// <summary>
    /// The page number for pagination. If not provided, it defaults to the first page.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// The number of rows to be returned in the result set. If not specified, the default value will be used.
    /// </summary>
    public int? RowCount { get; set; }

    /// <summary>
    /// The sorting order for the query. The format is typically "FieldName ASC" or "FieldName DESC".
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// The filters to be applied to the query, in string format. 
    /// This string represents a collection of filter conditions in a serialized format.
    /// </summary>
    public string? Filters { get; set; }
}
