using EasyEnglish.Core.Models;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace EasyEnglish.Services;

/// <summary>
/// Handles export and import of an entire <see cref="CourseModel"/> as a ZIP archive.
///
/// Archive layout:
/// <code>
/// my_course.zip
/// ├── course.json              ← CoursePackageManifest (includes export options)
/// ├── units/
/// │   ├── unit_1.json          ← UnitModel only (no options — they are global, stored once in course.json)
/// │   └── unit_2.json
/// └── audio/
///     ├── apple.mp3            ← stem = SanitizeFileName(word.Word)
///     └── banana.mp3
/// </code>
///
/// Options (includeExamples, includeLearningProgress, isFullBackup) are stored once
/// in course.json and never duplicated inside unit files.
/// NOTE: <see cref="UnitBackupService.ExportToJson"/> mutates the model it receives,
/// so this service serialises units via <see cref="BuildUnitDto"/> to keep
/// original models intact.
/// </summary>
public class CourseZipBackupService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    // =========================================================================
    // EXPORT
    // =========================================================================

    /// <summary>
    /// Packs <paramref name="course"/> and <paramref name="units"/> into a ZIP byte array.
    /// Audio bytes (<see cref="WordModel.Pronunciation"/>) go into separate
    /// <c>audio/*.mp3</c> entries. Original models are <b>not mutated</b>.
    /// </summary>
    public async Task<byte[]> ExportCourseToZipAsync(
        CourseModel course,
        IEnumerable<UnitModel> units,
        UnitBackupOptions options)
    {
        using var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var manifest = new CoursePackageManifest
            {
                CourseGuid              = course.RecordGuid,
                CourseTitle             = course.Title,
                CourseDescription       = course.Description,
                ExportedAt              = DateTime.UtcNow,
                Options = new CourseExportOptions
                {
                    IncludeExamples         = options.includeExamples,
                    IncludeLearningProgress = options.includeLearningProgress,
                    IsFullBackup            = options.isFullBackup,
                },
            };

            var writtenAudio = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int unitIndex = 1;

            foreach (var unit in units)
            {
                var unitEntry = new UnitManifestEntry
                {
                    UnitGuid  = unit.RecordGuid,
                    FileName  = $"units/unit_{unitIndex}.json",
                    Title     = unit.Title,
                    WordCount = unit.Words?.Count ?? 0,
                };

                // Write audio files (original model untouched).
                if (unit.Words is not null)
                {
                    foreach (var word in unit.Words.Where(w => w.Pronunciation is { Length: > 0 }))
                    {
                        var audioPath = $"audio/{SanitizeFileName(word.Word)}.mp3";
                        unitEntry.AudioFiles.Add(audioPath);

                        if (writtenAudio.Add(audioPath))
                            await WriteEntryAsync(archive, audioPath, word.Pronunciation!);
                    }
                }

                // Serialise UnitModel directly — options are global and live in course.json only.
                var dto      = BuildUnitDto(unit, options);
                var unitJson = JsonSerializer.Serialize(dto, JsonOpts);
                await WriteEntryAsync(archive, unitEntry.FileName, Encoding.UTF8.GetBytes(unitJson));

                manifest.Units.Add(unitEntry);
                unitIndex++;
            }

            // Manifest written last so unit list is complete.
            var manifestJson = JsonSerializer.Serialize(manifest, JsonOpts);
            await WriteEntryAsync(archive, "course.json", Encoding.UTF8.GetBytes(manifestJson));
        }

        return ms.ToArray();
    }

    // =========================================================================
    // IMPORT
    // =========================================================================

    /// <summary>
    /// Reads only <c>course.json</c> — fast, no unit parsing.
    /// Ideal for the preview step and duplicate-GUID detection.
    /// </summary>
    public async Task<CoursePackageManifest?> ReadManifestAsync(Stream zipStream)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        var entry = archive.GetEntry("course.json")
            ?? throw new InvalidDataException("Архів не містить файл course.json.");

        await using var stream = entry.Open();
        return await JsonSerializer.DeserializeAsync<CoursePackageManifest>(stream, JsonOpts);
    }

    /// <summary>
    /// Fully extracts all units with pronunciation bytes re-attached.
    /// <paramref name="zipStream"/> must be seekable.
    /// </summary>
    public async Task<CourseImportResult> ImportFromZipAsync(
        Stream zipStream,
        CoursePackageManifest manifest)
    {
        zipStream.Seek(0, SeekOrigin.Begin);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        // Pre-cache all audio keyed by archive path.
        var audioCache = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var ae in archive.Entries.Where(e =>
            e.FullName.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) &&
            e.FullName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)))
        {
            await using var s   = ae.Open();
            using var buf       = new MemoryStream();
            await s.CopyToAsync(buf);
            audioCache[ae.FullName] = buf.ToArray();
        }

        var options = new UnitBackupOptions
        {
            includeExamples         = manifest.Options.IncludeExamples,
            includeLearningProgress = manifest.Options.IncludeLearningProgress,
            isFullBackup            = manifest.Options.IsFullBackup,
        };

        var units = new List<(UnitManifestEntry Entry, UnitModel Unit)>();

        foreach (var unitEntry in manifest.Units)
        {
            var zipEntry = archive.GetEntry(unitEntry.FileName)
                ?? throw new InvalidDataException($"Відсутній файл: {unitEntry.FileName}");

            await using var us = zipEntry.Open();
            using var sr       = new StreamReader(us, Encoding.UTF8);
            var json = await sr.ReadToEndAsync();

            var unit = JsonSerializer.Deserialize<UnitModel>(json, JsonOpts)
                ?? throw new InvalidDataException($"Не вдалося розпарсити {unitEntry.FileName}");

            // Re-attach audio.
            if (unit.Words is not null)
            {
                foreach (var word in unit.Words)
                {
                    var path = $"audio/{SanitizeFileName(word.Word)}.mp3";
                    if (audioCache.TryGetValue(path, out var bytes))
                        word.Pronunciation = bytes;
                }
            }

            // GUID comes from the manifest (JSON payload may not carry it).
            unit.RecordGuid = unitEntry.UnitGuid;

            units.Add((unitEntry, unit));
        }

        return new CourseImportResult
        {
            Manifest = manifest,
            Options  = options,
            Units    = units,
        };
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    /// <summary>
    /// Creates a <see cref="UnitModel"/> DTO by projecting the source model
    /// according to <paramref name="options"/>. The source is never mutated.
    /// Pronunciation bytes are stripped (they live in separate audio entries).
    /// Options themselves are NOT included — they belong to course.json only.
    /// </summary>
    private static UnitModel BuildUnitDto(UnitModel source, UnitBackupOptions options)
    {
        var words = source.Words?
            .Select(w => new WordModel
            {
                Id            = options.isFullBackup ? w.Id : 0,
                Word          = w.Word,
                Transcription = w.Transcription,
                Translation   = w.Translation,
                Pronunciation = null,   // stored as audio file
                UnitId        = options.isFullBackup ? w.UnitId : 0,
                CreatedAt     = options.isFullBackup ? w.CreatedAt : DateTime.UtcNow,
                UpdatedAt     = options.isFullBackup ? w.UpdatedAt : null,
                LastReviewDate = options.includeLearningProgress ? w.LastReviewDate : null,
                ReviewCount   = options.includeLearningProgress ? w.ReviewCount : 0,
                Rate          = options.includeLearningProgress ? w.Rate : 3f,
                Examples      = options.includeExamples ? w.Examples : [],
            })
            .ToList();

        return new UnitModel
        {
            Id             = options.isFullBackup ? source.Id : 0,
            RecordGuid     = source.RecordGuid,
            Title          = source.Title,
            Description    = source.Description,
            Content        = source.Content,
            CourseId       = options.isFullBackup ? source.CourseId : 0,
            CreatedAt      = options.isFullBackup ? source.CreatedAt : DateTime.UtcNow,
            UpdatedAt      = options.isFullBackup ? source.UpdatedAt : null,
            LastReviewDate = options.includeLearningProgress ? source.LastReviewDate : null,
            ReviewCount    = options.includeLearningProgress ? source.ReviewCount : 0,
            Words          = words,
        };
    }

    /// <summary>
    /// Converts a word string into a safe ZIP path segment.
    /// Example: <c>"read (v.2)"</c> → <c>"read_v_2"</c>
    /// </summary>
    public static string SanitizeFileName(string word)
    {
        var s = Regex.Replace(word.Trim(), @"[^\w\-]", "_");
        s = Regex.Replace(s, @"_+", "_");
        return s.Trim('_').ToLowerInvariant();
    }

    private static async Task WriteEntryAsync(ZipArchive archive, string name, byte[] data)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        await using var stream = entry.Open();
        await stream.WriteAsync(data);
    }
}

// =========================================================================
// RESULT DTO
// =========================================================================

public class CourseImportResult
{
    public CoursePackageManifest Manifest { get; init; } = null!;
    public UnitBackupOptions     Options  { get; init; } = new();
    public List<(UnitManifestEntry Entry, UnitModel Unit)> Units { get; init; } = [];

    public int TotalWords      => Units.Sum(u => u.Unit.Words?.Count ?? 0);
    public int TotalAudioFiles => Units.Sum(u => u.Entry.AudioFiles.Count);
}
