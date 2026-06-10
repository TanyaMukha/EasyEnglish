using AutoMapper;

namespace EasyEnglish.Core.Mapping;

public static class MappingContextExtensions
{
    public static UnitMappingOptions? GetUnitOptions(this ResolutionContext context)
    {
        // TryGetItems не кидає виняток якщо Items недоступні
        if (context.TryGetItems(out var items) &&
            items.TryGetValue(UnitMappingOptions.Key, out var val))
            return (UnitMappingOptions)val;

        return null;
    }
}
