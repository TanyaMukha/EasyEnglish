namespace MukhaLab.SelectQueryParameters.Models;

public class SortDescriptor
{
    public string Field { get; init; } = default!;
    public SortDirection Direction { get; init; }
}
