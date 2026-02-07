using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;
using System.Text.Json.Serialization;

namespace EasyEnglish.Core.Models;

public class WordModel : AbstractModel, IReviewInfo, IRateInfo, IAuditInfo
{
    public string Word { get; set; } = string.Empty;

    public string? Transcription { get; set; }

    public string? Translation { get; set; }

    public byte[]? Pronunciation { get; set; }

    public DateTime? LastReviewDate { get; set; }

    public int ReviewCount { get; set; } = 0;

    public float Rate { get; set; } = 3; // Scale from 1 to 5, it's dificulty level

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    public int CourseId { get; set; }

    public int UnitId { get; set; }

    public IList<ExampleModel>? Examples { get; set; }

    [JsonIgnore]
    public UnitModel? Unit { get; set; }
}
