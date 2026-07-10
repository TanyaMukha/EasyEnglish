namespace MukhaLab.SelectQueryParameters.Models;

/// <summary>
/// Describes a single sort key. Multiple descriptors in <see cref="QueryParameters.Sort"/> are
/// applied in list order: the first becomes <c>OrderBy</c>/<c>OrderByDescending</c>, the rest
/// become <c>ThenBy</c>/<c>ThenByDescending</c>.
/// </summary>
public class SortDescriptor
{
    /// <summary>
    /// Property path to sort by, resolved the same way as <see cref="FilterValue.Field"/>
    /// (supports dot-separated navigation paths, e.g. "Task.Title").
    /// </summary>
    public string Field { get; init; } = default!;

    /// <summary>Sort direction for this key.</summary>
    public SortDirection Direction { get; init; }
}
