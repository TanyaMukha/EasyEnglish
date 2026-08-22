using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;
using EasyPeasy.Core.Interfaces.Fields;
using EasyPeasy.Core.Enums;

namespace EasyPeasy.Core.Entities;

/// <summary>
/// A non-graded study card belonging to a <see cref="UnitEntity"/>. Unlike <see cref="TestCardEntity"/>,
/// a study card has no correct/incorrect answer — it presents information for the learner to review.
/// Its field usage depends on <see cref="Kind"/>: see <see cref="Title"/> and <see cref="Body"/>.
/// </summary>
[Table("study_cards")]
public class StudyCardEntity : AbstractEntity, IReviewInfo, IRateInfo, IAuditInfo, IGuidRecord
{
    [Column("guid")]
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    [Column("kind")]
    public StudyCardKind Kind { get; set; }

    /// <summary>
    /// The term (<see cref="StudyCardKind.Term"/>), the heading (<see cref="StudyCardKind.Text"/>),
    /// or optional (<see cref="StudyCardKind.BlurredText"/>), depending on <see cref="Kind"/>.
    /// </summary>
    [NotNull]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The definition (<see cref="StudyCardKind.Term"/>), the main body text (<see cref="StudyCardKind.Text"/>),
    /// or text containing **blurred** segments (<see cref="StudyCardKind.BlurredText"/>), depending on <see cref="Kind"/>.
    /// </summary>
    [MaxLength(2000)]
    [Column("body")]
    public string? Body { get; set; }

    [MaxLength(2000)]
    [Column("dialogue")]
    public string? Dialogue { get; set; }

    [MaxLength(2000)]
    [Column("code_block")]
    public string? CodeBlock { get; set; }

    /// <summary>Only meaningful when <see cref="Kind"/> is <see cref="StudyCardKind.BlurredText"/>.</summary>
    [Column("reveal_mode")]
    public BlurRevealMode? RevealMode { get; set; }

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
