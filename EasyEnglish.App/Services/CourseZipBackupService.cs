using AutoMapper;
using EasyEnglish.App.Services;
using EasyEnglish.Core.Mapping;
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
/// │   ├── unit_1.json
/// │   └── unit_2.json
/// └── audio/
///     ├── apple.mp3            ← stem = SanitizeFileName(word.Word)
///     └── banana.mp3
/// </code>
///
/// Options (IncludeExamples, IncludeLearningProgress) are stored once in course.json. Unit files
/// never contain options — they always carry the full model snapshot produced by AutoMapper
/// according to those options.
///
/// Database IDs are never written: every model in the archive has Id/CourseId/UnitId/WordId at 0,
/// and identity travels as RecordGuid alone. Import resolves real local IDs by GUID
/// (see UnitService.ReconcileAndUpdateAsync), which is what makes an archive portable between app
/// instances whose IDs have nothing to do with each other.
/// </summary>
public class CourseZipBackupService
{
    private readonly IMapper _mapper;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented          = true,
        PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
        Encoder                = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters             = { new JsonStringEnumConverter() },
    };

    public CourseZipBackupService(IMapper mapper)
    {
        _mapper = mapper;
    }

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
        var mappingOptions = options.ToMappingOptions();

        using var ms = new MemoryStream();

        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var manifest = new CoursePackageManifest
            {
                CourseGuid        = course.RecordGuid,
                CourseTitle       = course.Title,
                CourseDescription = course.Description,
                LanguageCode      = course.LanguageCode,
                ExportedAt        = DateTime.UtcNow,
                Options = new CourseExportOptions
                {
                    IncludeExamples         = options.IncludeExamples,
                    IncludeLearningProgress = options.IncludeLearningProgress,
                },
            };

            var writtenAudio = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int unitIndex    = 1;

            foreach (var unit in units)
            {
                // Map to DTO — source model is never mutated.
                var dto = _mapper.Map<UnitModel>(unit, opts =>
                    opts.Items[UnitMappingOptions.Key] = mappingOptions);

                var unitEntry = new UnitManifestEntry
                {
                    UnitGuid  = unit.RecordGuid,    // always from original
                    FileName  = $"units/unit_{unitIndex}.json",
                    Title     = unit.Title,
                    WordCount = unit.Words?.Count ?? 0,
                };

                // Write audio from original model; strip bytes from DTO.
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

                // Pronunciation was already set to null by WordMappingAction
                // (Words are mapped via Model→Model, Pronunciation is not mapped).
                // Explicit safety-clear in case mapping config changes:
                if (dto.Words is not null)
                    foreach (var w in dto.Words)
                        w.Pronunciation = null;

                // Test cards have no stable content-derived key (unlike words), so images are
                // keyed by position within the unit's TestCards list instead.
                if (unit.TestCards is not null)
                {
                    for (var i = 0; i < unit.TestCards.Count; i++)
                    {
                        var image = unit.TestCards[i].Image;
                        if (image is not { Length: > 0 })
                            continue;

                        var imagePath = $"images/{unit.RecordGuid}_card{i}.{ImageDataUriHelper.DetectExtension(image)}";
                        unitEntry.ImageFiles.Add(imagePath);
                        await WriteEntryAsync(archive, imagePath, image);

                        if (dto.TestCards is not null && i < dto.TestCards.Count)
                            dto.TestCards[i].Image = null;
                    }
                }

                // Unit material (Content) goes into its own file, keyed by the unit's stable GUID
                // (its Title isn't guaranteed unique).
                if (!string.IsNullOrEmpty(unit.Content))
                {
                    var contentPath = $"content/{unit.RecordGuid}.html";
                    unitEntry.ContentFile = contentPath;
                    await WriteEntryAsync(archive, contentPath, Encoding.UTF8.GetBytes(unit.Content));
                    dto.Content = null;
                }

                var unitJson = JsonSerializer.Serialize(dto, JsonOpts);
                await WriteEntryAsync(archive, unitEntry.FileName, Encoding.UTF8.GetBytes(unitJson));

                manifest.Units.Add(unitEntry);
                unitIndex++;
            }

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
            ?? throw new InvalidDataException("The archive doesn't contain a course.json file.");

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
            e.FullName.EndsWith(".mp3",    StringComparison.OrdinalIgnoreCase)))
        {
            await using var s  = ae.Open();
            using var buf      = new MemoryStream();
            await s.CopyToAsync(buf);
            audioCache[ae.FullName] = buf.ToArray();
        }

        // Pre-cache all test card images, keyed by path without extension (the extension isn't
        // known ahead of time — it's whatever ImageDataUriHelper detected at export time).
        var imageCache = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var ie in archive.Entries.Where(e =>
            e.FullName.StartsWith("images/", StringComparison.OrdinalIgnoreCase)))
        {
            var dotIndex = ie.FullName.LastIndexOf('.');
            var key = dotIndex >= 0 ? ie.FullName[..dotIndex] : ie.FullName;

            await using var s = ie.Open();
            using var buf      = new MemoryStream();
            await s.CopyToAsync(buf);
            imageCache[key] = buf.ToArray();
        }

        var units = new List<(UnitManifestEntry Entry, UnitModel Unit)>();

        foreach (var unitEntry in manifest.Units)
        {
            var zipEntry = archive.GetEntry(unitEntry.FileName)
                ?? throw new InvalidDataException($"Missing file: {unitEntry.FileName}");

            await using var us = zipEntry.Open();
            using var sr       = new StreamReader(us, Encoding.UTF8);
            var json = await sr.ReadToEndAsync();

            var unit = JsonSerializer.Deserialize<UnitModel>(json, JsonOpts)
                ?? throw new InvalidDataException($"Failed to parse {unitEntry.FileName}");

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

            // Re-attach test card images. unitEntry.UnitGuid (not the deserialized unit.RecordGuid)
            // is authoritative — it's always the original GUID, even if the DTO's got regenerated.
            if (unit.TestCards is not null)
            {
                for (var i = 0; i < unit.TestCards.Count; i++)
                {
                    var key = $"images/{unitEntry.UnitGuid}_card{i}";
                    if (imageCache.TryGetValue(key, out var imageBytes))
                        unit.TestCards[i].Image = imageBytes;
                }
            }

            // Re-attach unit content. Absent entry (old archive, or a unit with no material)
            // leaves whatever the JSON deserialization produced untouched.
            var contentEntry = archive.GetEntry($"content/{unitEntry.UnitGuid}.html");
            if (contentEntry is not null)
            {
                await using var cs = contentEntry.Open();
                using var csr      = new StreamReader(cs, Encoding.UTF8);
                unit.Content = await csr.ReadToEndAsync();
            }

            // GUID comes from the manifest (authoritative source).
            unit.RecordGuid = unitEntry.UnitGuid;

            units.Add((unitEntry, unit));
        }

        return new CourseImportResult
        {
            Manifest = manifest,
            Units    = units,
        };
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

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

/// <summary>Result of <see cref="CourseZipBackupService.ImportFromZipAsync"/> — fully-materialized units plus the manifest they came from.</summary>
public class CourseImportResult
{
    public CoursePackageManifest                           Manifest { get; init; } = null!;
    public List<(UnitManifestEntry Entry, UnitModel Unit)> Units    { get; init; } = [];

    public int TotalWords      => Units.Sum(u => u.Unit.Words?.Count ?? 0);
    public int TotalAudioFiles => Units.Sum(u => u.Entry.AudioFiles.Count);
    public int TotalImageFiles => Units.Sum(u => u.Entry.ImageFiles.Count);
}
