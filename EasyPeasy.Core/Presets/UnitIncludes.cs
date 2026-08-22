namespace EasyPeasy.Core.Presets;

/// <summary>
/// Named presets of EF Core navigation-property paths for <c>UnitEntity</c> — see
/// <see cref="CourseIncludes"/> for the caveat about these being plain, unchecked strings.
/// </summary>
public static class UnitIncludes
{
    public static readonly string[] None = Array.Empty<string>();
    public static readonly string[] WordsOnly = { "Words" };
    public static readonly string[] Full = { "Words", "Course", "IrregularForms", "Words.Examples", "StudyCards", "TestCards" };
}
