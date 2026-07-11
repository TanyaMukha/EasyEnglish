using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;
using System.Text.Json.Serialization;

namespace EasyEnglish.Core.Models;

/// <summary>
/// DTO for <see cref="EasyEnglish.Core.Entities.TestCardEntity"/>. Packed/unpacked by
/// <see cref="EasyEnglish.Core.Mapping.TestCardEntityToModelConverter"/> and
/// <see cref="EasyEnglish.Core.Mapping.TestCardModelToEntityConverter"/> — exactly one of
/// <see cref="Choice"/>, <see cref="ShortAnswer"/>, <see cref="Cloze"/>, <see cref="Matching"/> is
/// non-null, chosen by <see cref="Kind"/>.
/// </summary>
public class TestCardModel : AbstractModel, IReviewInfo, IRateInfo, IAuditInfo, IGuidRecord
{
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    public TestCardKind Kind { get; set; }

    public string? Title { get; set; }

    /// <summary>Optional short task/question text.</summary>
    public string? Question { get; set; }

    public string? Hint { get; set; }

    /// <summary>Optional illustrative image, stored as raw bytes (no separate MIME column).</summary>
    public byte[]? Image { get; set; }

    /// <summary>
    /// Optional formatted content: a reading passage, code snippet, etc. — or, for <see cref="TestCardKind.Cloze"/>,
    /// the cloze template with {0}, {1}, ... placeholders. A card may have <see cref="Question"/>, this, both, or neither.
    /// </summary>
    public string? FormattedText { get; set; }

    /// <summary>Optional explanation shown to the learner only after an incorrect answer.</summary>
    public string? Explanation { get; set; }

    // Filled in by the mapper based on Kind — exactly one of these four is non-null.
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
