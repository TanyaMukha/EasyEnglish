using AutoMapper;

namespace EasyPeasy.Core.Mapping;

/// <summary>Helpers for reading <see cref="UnitMappingOptions"/> out of an AutoMapper <see cref="ResolutionContext"/>.</summary>
public static class MappingContextExtensions
{
    /// <summary>
    /// Retrieves the <see cref="UnitMappingOptions"/> passed to <c>Map(source, opts => opts.Items[UnitMappingOptions.Key] = ...)</c>,
    /// or <c>null</c> if none were supplied (e.g. a plain map with no <c>Items</c> at all, or an unrelated call).
    /// </summary>
    public static UnitMappingOptions? GetUnitOptions(this ResolutionContext context)
    {
        // TryGetItems doesn't throw when no Items were passed to this particular Map call
        if (context.TryGetItems(out var items) &&
            items.TryGetValue(UnitMappingOptions.Key, out var val))
            return (UnitMappingOptions)val;

        return null;
    }
}
