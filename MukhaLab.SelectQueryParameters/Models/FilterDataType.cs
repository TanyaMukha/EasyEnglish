namespace MukhaLab.SelectQueryParameters.Models;

/// <summary>
/// Identifies the CLR type that a raw filter value (<see cref="FilterParameter.Value"/>,
/// <see cref="FilterParameter.From"/>, <see cref="FilterParameter.To"/>) should be converted to
/// before it is embedded into the generated expression tree.
/// </summary>
/// <remarks>
/// The value is always received as a boxed <see cref="object"/> (typically a string coming from
/// a query string or a JSON payload). <c>QueryHelperExtensions.ConvertFilterValue</c> uses this
/// enum to pick the correct <c>Convert.ToXxx</c> / <c>Parse</c> call.
/// </remarks>
public enum FilterDataType
{
    /// <summary>The value is used as-is (<see cref="string"/>).</summary>
    String,

    /// <summary>The value is converted with <see cref="Convert.ToInt32(string?)"/>.</summary>
    Integer,

    /// <summary>The value is converted with <see cref="Convert.ToDecimal(string?)"/>.</summary>
    Decimal,

    /// <summary>The value is converted with <see cref="Convert.ToDateTime(string?)"/>.</summary>
    DateTime,

    /// <summary>The value is converted with <see cref="Convert.ToDateTime(string?)"/> and truncated to its date component.</summary>
    Date,

    /// <summary>The value is converted with <see cref="Convert.ToBoolean(string?)"/>.</summary>
    Boolean,

    /// <summary>The value is converted with <see cref="Guid.Parse(string)"/>.</summary>
    Guid
}
