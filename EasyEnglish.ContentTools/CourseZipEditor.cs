using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using EasyEnglish.Core.Models;

namespace EasyEnglish.ContentTools;

/// <summary>
/// Reads/writes a single <c>unit_N.json</c> inside an exported course-ZIP archive (the format
/// produced by <c>EasyEnglish.App/Services/CourseZipBackupService.cs</c>), without touching the
/// rest of the archive (words, audio, <c>course.json</c>). Meant for manually/programmatically
/// adding <c>StudyCard</c>/<c>TestCard</c> entries to an already-exported module without running
/// the app itself.
/// </summary>
public static class CourseZipEditor
{
    /// <summary>
    /// Must match <c>EasyEnglish.App/Services/CourseZipBackupService.JsonOpts</c> exactly, or the
    /// app won't be able to read the file back.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Reads and deserializes <paramref name="unitFileName"/> (e.g. <c>"units/unit_1.json"</c>) from inside the ZIP at <paramref name="zipPath"/>.</summary>
    /// <exception cref="InvalidOperationException">The entry is missing or fails to parse.</exception>
    public static UnitModel LoadUnit(string zipPath, string unitFileName)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry(unitFileName)
            ?? throw new InvalidOperationException($"{unitFileName} не знайдено в {zipPath}");

        using var stream = entry.Open();
        return JsonSerializer.Deserialize<UnitModel>(stream, JsonOpts)
            ?? throw new InvalidOperationException($"Не вдалося розпарсити {unitFileName}");
    }

    /// <summary>Overwrites <paramref name="unitFileName"/> inside an already-existing (copied) archive.</summary>
    public static void SaveUnit(string zipPath, string unitFileName, UnitModel unit)
    {
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

        archive.GetEntry(unitFileName)?.Delete();

        var entry = archive.CreateEntry(unitFileName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        var json = JsonSerializer.Serialize(unit, JsonOpts);
        stream.Write(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>Copies a course-ZIP archive, overwriting <paramref name="targetPath"/> if it already exists — the usual first step before editing a unit in place.</summary>
    public static void CopyArchive(string sourcePath, string targetPath) =>
        File.Copy(sourcePath, targetPath, overwrite: true);

    // CoursePackageManifest (course.json's C# model) lives in EasyEnglish.App, which this
    // console project doesn't reference (MAUI multi-targeting). We patch course.json as a
    // raw JsonNode tree instead — only the "units" array needs touching here.

    /// <summary>Reads <c>course.json</c> from inside the ZIP at <paramref name="zipPath"/> as a raw JSON tree (see the remark above on why not a typed model).</summary>
    /// <exception cref="InvalidOperationException"><c>course.json</c> is missing or fails to parse.</exception>
    public static JsonNode LoadManifestNode(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry("course.json")
            ?? throw new InvalidOperationException($"course.json не знайдено в {zipPath}");

        using var stream = entry.Open();
        return JsonNode.Parse(stream)
            ?? throw new InvalidOperationException("Не вдалося розпарсити course.json");
    }

    /// <summary>Overwrites <c>course.json</c> — used after adding a new unit file to the archive, since the manifest is the only place that lists which unit files exist.</summary>
    public static void SaveManifestNode(string zipPath, JsonNode manifest)
    {
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

        archive.GetEntry("course.json")?.Delete();

        var entry = archive.CreateEntry("course.json", CompressionLevel.Optimal);
        using var stream = entry.Open();
        var json = manifest.ToJsonString(JsonOpts);
        stream.Write(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>Lists every <c>units/*.json</c> entry inside the archive at <paramref name="zipPath"/>, sorted by file name.</summary>
    public static List<string> ListUnitFiles(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        return archive.Entries
            .Where(e => e.FullName.StartsWith("units/", StringComparison.OrdinalIgnoreCase) && e.FullName.EndsWith(".json"))
            .Select(e => e.FullName)
            .OrderBy(f => f)
            .ToList();
    }
}
