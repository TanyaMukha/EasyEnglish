using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;
using System.Text.Json.Serialization;

namespace EasyEnglish.Core.Models;

public class TestCardModel : AbstractModel, IReviewInfo, IRateInfo, IAuditInfo
{
    public TestCardKind Kind { get; set; }

    public string? Title { get; set; }

    public string Text { get; set; } = string.Empty;

    // Заповнюється мапером за Kind — не-null рівно одна з чотирьох властивостей.
    public ChoicePayload? Choice { get; set; }

    public ShortAnswerPayload? ShortAnswer { get; set; }

    public ClozePayload? Cloze { get; set; }

    public MatchingPayload? Matching { get; set; }

    public DateTime? LastReviewDate { get; set; }

    public int ReviewCount { get; set; } = 0;

    public float Rate { get; set; } = 3;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int UnitId { get; set; }

    [JsonIgnore]
    public UnitModel? Unit { get; set; }
}
