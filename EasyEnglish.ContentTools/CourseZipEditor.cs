using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using EasyEnglish.Core.Models;

namespace EasyEnglish.ContentTools;

/// <summary>
/// Читає/пише один unit_N.json усередині експортованого courseZip-архіву
/// (формат — EasyEnglish.App/Services/CourseZipBackupService.cs), не чіпаючи
/// решту архіву (слова, аудіо, course.json). Призначено для ручного/скриптового
/// додавання StudyCard/TestCard у вже експортований модуль, без запуску застосунку.
/// </summary>
public static class CourseZipEditor
{
    // Має точно збігатися з EasyEnglish.App/Services/CourseZipBackupService.JsonOpts,
    // інакше застосунок не зможе прочитати файл назад.
    public static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    public static UnitModel LoadUnit(string zipPath, string unitFileName)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry(unitFileName)
            ?? throw new InvalidOperationException($"{unitFileName} не знайдено в {zipPath}");

        using var stream = entry.Open();
        return JsonSerializer.Deserialize<UnitModel>(stream, JsonOpts)
            ?? throw new InvalidOperationException($"Не вдалося розпарсити {unitFileName}");
    }

    /// <summary>Перезаписує unit_N.json усередині вже наявного (скопійованого) архіву.</summary>
    public static void SaveUnit(string zipPath, string unitFileName, UnitModel unit)
    {
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

        archive.GetEntry(unitFileName)?.Delete();

        var entry = archive.CreateEntry(unitFileName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        var json = JsonSerializer.Serialize(unit, JsonOpts);
        stream.Write(Encoding.UTF8.GetBytes(json));
    }

    public static void CopyArchive(string sourcePath, string targetPath) =>
        File.Copy(sourcePath, targetPath, overwrite: true);

    // CoursePackageManifest (course.json's C# model) lives in EasyEnglish.App, which this
    // console project doesn't reference (MAUI multi-targeting). We patch course.json as a
    // raw JsonNode tree instead — only the "units" array needs touching here.

    public static JsonNode LoadManifestNode(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry("course.json")
            ?? throw new InvalidOperationException($"course.json не знайдено в {zipPath}");

        using var stream = entry.Open();
        return JsonNode.Parse(stream)
            ?? throw new InvalidOperationException("Не вдалося розпарсити course.json");
    }

    /// <summary>Перезаписує course.json — використовується після додавання нового unit-файлу в архів.</summary>
    public static void SaveManifestNode(string zipPath, JsonNode manifest)
    {
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

        archive.GetEntry("course.json")?.Delete();

        var entry = archive.CreateEntry("course.json", CompressionLevel.Optimal);
        using var stream = entry.Open();
        var json = manifest.ToJsonString(JsonOpts);
        stream.Write(Encoding.UTF8.GetBytes(json));
    }

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
