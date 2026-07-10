namespace MukhaLab.SelectQueryParameters.Models;

/// <summary>
/// Concrete filter description consumed by <c>QueryHelperExtensions</c>. Combines the property
/// path and operation inherited from <see cref="FilterValue"/> with the raw comparison value(s).
/// </summary>
/// <remarks>
/// <see cref="Value"/>, <see cref="From"/>, and <see cref="To"/> are untyped on purpose: they are
/// typically deserialized from a query string or a JSON request body, so the caller does not know
/// the target CLR type at binding time. Set <see cref="FilterValue.DataType"/> to tell the engine
/// how to convert them. Use <see cref="Value"/> for single-value operations
/// (<see cref="FilterOperation.Equal"/>, <see cref="FilterOperation.Contains"/>, etc.) and
/// <see cref="From"/>/<see cref="To"/> for <see cref="FilterOperation.Between"/>.
/// </remarks>
public class FilterParameter : FilterValue
{
    /// <summary>Upper bound used by <see cref="FilterOperation.Between"/>.</summary>
    public object To { get; set; } = null!;

    /// <summary>Comparison value used by every operation except <see cref="FilterOperation.Between"/>, <see cref="FilterOperation.IsNull"/>, and <see cref="FilterOperation.IsNotNull"/>.</summary>
    public object Value { get; set; } = null!;

    /// <summary>Lower bound used by <see cref="FilterOperation.Between"/>.</summary>
    public object From { get; set; } = null!;

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.Field.ToString();
    }
}
