namespace EasyEnglish.Core.Presets;

/// <summary>
/// Named presets of EF Core navigation-property paths for <c>IrregularFormEntity</c> — see
/// <see cref="CourseIncludes"/> for the caveat about these being plain, unchecked strings.
/// </summary>
public static class IrregularFormIncludes
{
    public static readonly string[] None = Array.Empty<string>();
    public static readonly string[] Full = { "Unit" };
}
