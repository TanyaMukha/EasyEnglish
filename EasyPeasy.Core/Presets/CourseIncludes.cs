namespace EasyPeasy.Core.Presets;

/// <summary>
/// Named presets of EF Core navigation-property paths for <c>CourseEntity</c>, passed as the
/// <c>includes</c> parameter to repository/service methods (see <see cref="MukhaLab.Database.IBaseRepository{T}"/>).
/// These are plain strings, not compiler-checked against the entity's actual navigation properties —
/// a rename that isn't mirrored here fails at runtime (EF Core throws on an unresolvable path), not at compile time.
/// </summary>
public static class CourseIncludes
{
    public static readonly string[] None = Array.Empty<string>();
    public static readonly string[] Full = { "Units", "Subject" };
}
