using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;
using EasyPeasy.Core.Interfaces.Fields;

namespace EasyPeasy.Core.Entities;

/// <summary>
/// A lesson/module within a <see cref="CourseEntity"/>. Owns the four kinds of learning content:
/// <see cref="WordEntity"/>, <see cref="IrregularFormEntity"/>, <see cref="StudyCardEntity"/>, and
/// <see cref="TestCardEntity"/>.
/// </summary>
[Table("units")]
public class UnitEntity : AbstractEntity, IReviewInfo, IAuditInfo, IGuidRecord
{
    [Column("guid")]
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    [NotNull]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("content")]
    public string? Content { get; set; }

    [Column("last_review_date")]
    public DateTime? LastReviewDate { get; set; }

    [Column("review_count")]
    public int ReviewCount { get; set; } = 0;

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

    [ForeignKey("Course")]
    [Column("course_id")]
    public int CourseId { get; set; }

    public List<WordEntity> Words { get; set; } = new();

    public List<IrregularFormEntity> IrregularForms { get; set; } = new();

    public List<StudyCardEntity> StudyCards { get; set; } = new();

    public List<TestCardEntity> TestCards { get; set; } = new();

    public CourseEntity? Course { get; set; }
}
