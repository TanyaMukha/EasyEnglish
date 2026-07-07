namespace EasyEnglish.Core.Models;

/// <summary>
/// Root manifest stored as <c>course.json</c> inside the ZIP archive.
/// Carries course metadata, export options (once, globally), and an ordered
/// list of unit entries.
/// </summary>
public class CoursePackageManifest
{
    /// <summary>Format version — increment on breaking schema changes.</summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>
    /// Stable identifier of the course.
    /// Used to detect whether the archive has already been imported
    /// and offer an "update existing" flow instead of creating a duplicate.
    /// </summary>
    public Guid CourseGuid { get; set; } = Guid.NewGuid();

    public string  CourseTitle       { get; set; } = string.Empty;
    public string? CourseDescription { get; set; }

    /// <summary>Мова курсу у форматі BCP-47 (наприклад, "en-us").</summary>
    public string? LanguageCode { get; set; } = "en-us";

    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Application name / version that produced this archive.</summary>
    public string ExportedBy { get; set; } = "EasyEnglish";

    /// <summary>
    /// Options that were active during export.
    /// Stored once here — never duplicated inside individual unit files.
    /// Used as defaults when the user opens the import screen.
    /// </summary>
    public CourseExportOptions Options { get; set; } = new();

    public List<UnitManifestEntry> Units { get; set; } = [];
}

/// <summary>
/// Export / import options stored once in <c>course.json</c>.
///
/// Resulting <c>course.json</c> fragment:
/// <code>
/// "options": {
///   "includeExamples": true,
///   "includeLearningProgress": true,
///   "isFullBackup": false
/// }
/// </code>
/// </summary>
public class CourseExportOptions
{
    public bool IncludeExamples         { get; set; } = true;
    public bool IncludeLearningProgress { get; set; } = true;
    public bool IsFullBackup            { get; set; } = false;
}

/// <summary>Describes a single unit inside the archive.</summary>
public class UnitManifestEntry
{
    /// <summary>
    /// Stable GUID of the unit.
    /// Allows detecting existing units on re-import so they can be updated
    /// rather than duplicated.
    /// </summary>
    public Guid UnitGuid { get; set; }

    /// <summary>Relative path inside the ZIP, e.g. <c>units/unit_1.json</c>.</summary>
    public string FileName { get; set; } = string.Empty;

    public string Title     { get; set; } = string.Empty;
    public int    WordCount { get; set; }

    /// <summary>
    /// Relative paths of audio files that belong to this unit,
    /// e.g. <c>["audio/apple.mp3", "audio/banana.mp3"]</c>.
    /// The stem equals <see cref="CourseZipBackupService.SanitizeFileName"/>(word).
    /// </summary>
    public List<string> AudioFiles { get; set; } = [];

    /// <summary>
    /// Relative paths of test card image files that belong to this unit, informational only
    /// (import re-derives the path from the card's position, it doesn't read this list back).
    /// </summary>
    public List<string> ImageFiles { get; set; } = [];

    /// <summary>Relative path of this unit's Content (HTML material) file, if it had any.</summary>
    public string? ContentFile { get; set; }
}
