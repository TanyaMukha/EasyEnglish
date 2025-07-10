namespace MukhaLib.SelectQueryParameters.Models;

/// <summary>
/// Base class for filter values.
/// </summary>
public abstract class FilterValue
{
    public string Field { get; set; } = string.Empty; // Title, Task.Title, Executors[].Title
    public FilterOperation Operation { get; set; } = FilterOperation.Equal;
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
