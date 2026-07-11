using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MukhaLab.Database;
using EasyEnglish.Core.Interfaces.Fields;
using EasyEnglish.Core.Enums;

namespace EasyEnglish.Core.Entities;

/// <summary>
/// A graded test card belonging to a <see cref="UnitEntity"/>. <see cref="Options"/> and
/// <see cref="CorrectAnswers"/> are opaque JSON columns whose shape depends on <see cref="Kind"/> —
/// see <see cref="EasyEnglish.Core.Mapping.TestCardEntityToModelConverter"/> for the packing format
/// of each <see cref="TestCardKind"/>, and the typed <c>EasyEnglish.Core.Models.TestCards.*Payload</c>
/// classes for the corresponding model-side shape.
/// </summary>
[Table("test_cards")]
public class TestCardEntity : AbstractEntity, IReviewInfo, IRateInfo, IAuditInfo, IGuidRecord
{
    [Column("guid")]
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    [Column("kind")]
    public TestCardKind Kind { get; set; }

    [MaxLength(200)]
    [Column("title")]
    public string? Title { get; set; }

    /// <summary>Optional short task/question text.</summary>
    [MaxLength(2000)]
    [Column("text")]
    public string? Question { get; set; }

    [MaxLength(500)]
    [Column("hint")]
    public string? Hint { get; set; }

    /// <summary>Optional illustrative image, stored as raw bytes (no separate MIME column).</summary>
    [Column("image")]
    public byte[]? Image { get; set; }

    /// <summary>
    /// Optional formatted content: a reading passage, code snippet, etc. — or, for <see cref="TestCardKind.Cloze"/>,
    /// the cloze template with {0}, {1}, ... placeholders. A card may have <see cref="Question"/>, this, both, or neither.
    /// </summary>
    [MaxLength(4000)]
    [Column("formatted_text")]
    public string? FormattedText { get; set; }

    /// <summary>Optional explanation shown to the learner only after an incorrect answer.</summary>
    [MaxLength(1000)]
    [Column("explanation")]
    public string? Explanation { get; set; }

    /// <summary>JSON; shape depends on <see cref="Kind"/> — see the class summary.</summary>
    [MaxLength(1000)]
    [Column("options")]
    public string? Options { get; set; }

    /// <summary>JSON; shape depends on <see cref="Kind"/> — see the class summary.</summary>
    [MaxLength(1000)]
    [Column("correct_answers")]
    public string? CorrectAnswers { get; set; }

    [Column("last_review_date")]
    public DateTime? LastReviewDate { get; set; }

    [Column("review_count")]
    public int ReviewCount { get; set; } = 0;

    [Column("rate")]
    public float Rate { get; set; } = 3;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp, if the entity has been modified.
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("Unit")]
    [Column("unit_id")]
    public int UnitId { get; set; }

    public UnitEntity? Unit { get; set; }
}
