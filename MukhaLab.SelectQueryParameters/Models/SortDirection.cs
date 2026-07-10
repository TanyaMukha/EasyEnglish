namespace MukhaLab.SelectQueryParameters.Models;

/// <summary>
/// Sort direction for a single <see cref="SortDescriptor"/>.
/// </summary>
public enum SortDirection
{
    /// <summary>Ascending order (<c>OrderBy</c> / <c>ThenBy</c>).</summary>
    Asc,

    /// <summary>Descending order (<c>OrderByDescending</c> / <c>ThenByDescending</c>).</summary>
    Desc
}
