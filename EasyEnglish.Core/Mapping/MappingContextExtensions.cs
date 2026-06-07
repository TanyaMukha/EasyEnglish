using AutoMapper;

namespace EasyEnglish.Core.Mapping;

public static class MappingContextExtensions
{
    public static UnitMappingOptions? GetUnitOptions(this ResolutionContext context) =>
        context.Items.TryGetValue(UnitMappingOptions.Key, out var val)
            ? (UnitMappingOptions)val
            : null;
}
