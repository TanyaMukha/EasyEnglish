using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;
using EasyEnglish.Core.Interfaces.Fields;
using EasyEnglish.Core.Enums;

namespace EasyEnglish.Core.Entities;

[Table("study_cards")]
public class StudyCardEntity : AbstractEntity, IReviewInfo, IRateInfo, IAuditInfo, IGuidRecord
{
    [Column("guid")]
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    [Column("kind")]
    public StudyCardKind Kind { get; set; }

    [NotNull]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;   // = Term (Kind.Term) / заголовок (Kind.Text) / необов'язково (Kind.BlurredText)

    [MaxLength(2000)]
    [Column("body")]
    public string? Body { get; set; }                    // = Definition (Kind.Term) / основний текст / текст з **розмитим**

    [MaxLength(2000)]
    [Column("dialogue")]
    public string? Dialogue { get; set; }

    [MaxLength(2000)]
    [Column("code_block")]
    public string? CodeBlock { get; set; }

    [Column("reveal_mode")]
    public BlurRevealMode? RevealMode { get; set; }       // тільки для Kind.BlurredText

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
