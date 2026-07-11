using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;
using System.Text.Json.Serialization;

namespace EasyEnglish.Core.Models;

/// <summary>DTO for <see cref="EasyEnglish.Core.Entities.StudyCardEntity"/>.</summary>
public class StudyCardModel : AbstractModel, IReviewInfo, IRateInfo, IAuditInfo, IGuidRecord
{
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    public StudyCardKind Kind { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public string? Dialogue { get; set; }

    public string? CodeBlock { get; set; }

    public BlurRevealMode? RevealMode { get; set; }

    public DateTime? LastReviewDate { get; set; }

    public int ReviewCount { get; set; } = 0;

    public float Rate { get; set; } = 3;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int UnitId { get; set; }

    [JsonIgnore]
    public UnitModel? Unit { get; set; }
}
