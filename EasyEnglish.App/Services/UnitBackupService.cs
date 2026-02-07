namespace EasyEnglish.Services;

using System.Text.Json;
using System.Text.Json.Serialization;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Models;

/// <summary>
/// Service for importing and exporting dictionary data in JSON format.
/// </summary>
internal class UnitBackupService
{
    private readonly JsonSerializerOptions jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitBackupService"/> class.
    /// Configures JSON serialization options with camel case naming and enum converters.
    /// </summary>
    public UnitBackupService()
    {
        this.jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(),
            },
        };
    }

    /// <summary>
    /// Export word list to JSON string.
    /// </summary>
    /// <param name="unit">Колекція елементів набору.</param>
    /// <returns>JSON рядок.</returns>
    public string ExportToJson(UnitModel unit, UnitBackupOptions? options = null)
    {
        try
        {
            if (options is not null)
            {
                if (!options.isFullBackup)
                {
                    unit.ClearKeyFields();
                }
                if (!options.includeExamples)
                {
                    unit.RemoveExamples();
                }
                if (!options.includeLearningProgress)
                {
                    unit.ClearLearningProgress();
                }
            }
            
            return JsonSerializer.Serialize(new UnitBackup(unit, options ?? new UnitBackupOptions()), this.jsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Помилка при експорті в JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Імпорт словника з JSON рядка
    /// </summary>
    /// <param name="json">JSON рядок</param>
    /// <returns>Колекція елементів словника</returns>
    public UnitBackup? ImportFromJson(string json)
    {
        try
        {
            var exportData = JsonSerializer.Deserialize<UnitBackup>(json, this.jsonOptions);

            return exportData;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Помилка при парсингу JSON: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Помилка при імпорті з JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Валідація JSON перед імпортом
    /// </summary>
    /// <param name="json">JSON рядок</param>
    /// <returns>True якщо JSON валідний</returns>
    public bool ValidateJson(string json)
    {
        try
        {
            JsonSerializer.Deserialize<UnitBackup>(json, this.jsonOptions);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class UnitBackupOptions
{
    public bool includeExamples { get; set; } = true;

    public bool includeLearningProgress { get; set; } = true;

    public bool isFullBackup { get; set; } = false;
}

public class UnitBackup
{
    public UnitBackup(UnitModel unit, UnitBackupOptions options)
    {
        Options = options;
        Unit = unit;
    }
    
    public UnitBackupOptions Options { get; set; }

    public UnitModel Unit { get; set; }
}