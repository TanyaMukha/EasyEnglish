namespace MukhaLib.SelectQueryParameters.Models;

/// <summary>
/// Represents a filtering operation.
/// </summary>
public enum FilterOperation
{
    Equal,              // Equal
    NotEqual,           // Not equal
    GreaterThan,        // Greater than
    GreaterThanOrEqual, // Greater than or equal
    LessThan,           // Less than
    LessThanOrEqual,    // Less than or equal
    Contains,           // Contains (for strings)
    StartsWith,         // Starts with (for strings)
    EndsWith,           // Ends with (for strings)
    Between,            // Between (for ranges)
    IsNull,             // NULL
    IsNotNull           // Not NULL
}
