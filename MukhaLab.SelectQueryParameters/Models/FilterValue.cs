namespace MukhaLab.SelectQueryParameters.Models;

/// <summary>
/// Base class for filter values.
/// </summary>
public abstract class FilterValue
{
    /// <summary>
    /// Property path the filter applies to, resolved by <c>QueryHelperExtensions.GetPropertyExpression</c>.
    /// Supports three forms: a simple property ("Title"), a dot-separated navigation path
    /// ("Task.Title"), and a collection existence check using square brackets
    /// ("Executors[Title]", meaning "does the Executors collection contain an item whose
    /// Title is not null"). See the library README for the collection-filter caveat.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Comparison to apply between the resolved property and the filter value(s).</summary>
    public FilterOperation Operation { get; set; } = FilterOperation.Equal;

    /// <summary>CLR type that the raw filter value(s) should be converted to before comparison.</summary>
    public FilterDataType DataType { get; set; } = FilterDataType.String;

    /// <summary>
    /// Checks whether the filter is valid.
    /// </summary>
    /// <returns>true if the filter is valid; otherwise false</returns>
    public virtual bool IsValid()
    {
        return !string.IsNullOrEmpty(this.Field);
    }
}
