namespace MukhaLab.SelectQueryParameters.Models;

/// <summary>
/// Identifies the comparison that <c>QueryHelperExtensions</c> should compile into an expression
/// tree for a given <see cref="FilterParameter"/>.
/// </summary>
public enum FilterOperation
{
    /// <summary>Property equals <see cref="FilterParameter.Value"/> (<c>==</c>).</summary>
    Equal,

    /// <summary>Property does not equal <see cref="FilterParameter.Value"/> (<c>!=</c>).</summary>
    NotEqual,

    /// <summary>Property is greater than <see cref="FilterParameter.Value"/> (<c>&gt;</c>). Requires a comparable type.</summary>
    GreaterThan,

    /// <summary>Property is greater than or equal to <see cref="FilterParameter.Value"/> (<c>&gt;=</c>). Requires a comparable type.</summary>
    GreaterThanOrEqual,

    /// <summary>Property is less than <see cref="FilterParameter.Value"/> (<c>&lt;</c>). Requires a comparable type.</summary>
    LessThan,

    /// <summary>Property is less than or equal to <see cref="FilterParameter.Value"/> (<c>&lt;=</c>). Requires a comparable type.</summary>
    LessThanOrEqual,

    /// <summary>String property contains <see cref="FilterParameter.Value"/> (<see cref="string.Contains(string)"/>). String properties only.</summary>
    Contains,

    /// <summary>String property starts with <see cref="FilterParameter.Value"/> (<see cref="string.StartsWith(string)"/>). String properties only.</summary>
    StartsWith,

    /// <summary>String property ends with <see cref="FilterParameter.Value"/> (<see cref="string.EndsWith(string)"/>). String properties only.</summary>
    EndsWith,

    /// <summary>Property is within the inclusive range [<see cref="FilterParameter.From"/>, <see cref="FilterParameter.To"/>].</summary>
    Between,

    /// <summary>Property is <c>null</c>. Only meaningful for reference types and nullable value types.</summary>
    IsNull,

    /// <summary>Property is not <c>null</c>. Only meaningful for reference types and nullable value types.</summary>
    IsNotNull
}
