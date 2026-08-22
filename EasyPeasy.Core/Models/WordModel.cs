using EasyPeasy.Core.Interfaces.Fields;
using MukhaLab.Database;
using System.Text.Json.Serialization;

namespace EasyPeasy.Core.Models;

/// <summary>DTO for <see cref="EasyPeasy.Core.Entities.WordEntity"/>.</summary>
public class WordModel : AbstractModel, IReviewInfo, IRateInfo, IAuditInfo, IGuidRecord
{
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    public string Word { get; set; } = string.Empty;

    public string? Transcription { get; set; }

    public string? Translation { get; set; }

    /// <summary>
    /// Додаткова примітка до слова (найчастіше позначається у дужках).
    /// Не показується в режимі навчання.
    /// </summary>
    public string? Note { get; set; }

    public byte[]? Pronunciation { get; set; }

    public DateTime? LastReviewDate { get; set; }

    public int ReviewCount { get; set; } = 0;

    public float Rate { get; set; } = 3; // Scale from 1 to 5, it's dificulty level

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    /// <remarks>Denormalized from <c>Unit.CourseId</c> by the entity→model map — not a real column on <c>words</c>.</remarks>
    [JsonIgnore]
    public int CourseId { get; set; }

    public int UnitId { get; set; }

    public IList<ExampleModel>? Examples { get; set; }

    [JsonIgnore]
    public UnitModel? Unit { get; set; }
}
