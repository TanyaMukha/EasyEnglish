namespace MukhaLib.SelectQueryParameters.Models;

/// <summary>
/// Represents a filtering parameter.
/// </summary>
public class FilterParameter : FilterValue
{
    public object To { get; set; } = null!;
    public object Value { get; set; } = null!;
    public object From { get; set; } = null!;

    public override string ToString()
    {
        return this.Field.ToString();
    }
}
