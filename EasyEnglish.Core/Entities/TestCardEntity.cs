using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;
using EasyEnglish.Core.Interfaces.Fields;
using EasyEnglish.Core.Enums;

namespace EasyEnglish.Core.Entities;

[Table("test_cards")]
public class TestCardEntity : AbstractEntity, IReviewInfo, IRateInfo, IAuditInfo
{
    [Column("kind")]
    public TestCardKind Kind { get; set; }

    [MaxLength(200)]
    [Column("title")]
    public string? Title { get; set; }

    [NotNull]
    [MaxLength(2000)]
    [Column("text")]
    public string Text { get; set; } = string.Empty;      // питання, або cloze-шаблон з {0},{1}...

    [MaxLength(500)]
    [Column("hint")]
    public string? Hint { get; set; }

    [MaxLength(1000)]
    [Column("options")]
    public string? Options { get; set; }                   // JSON, структура залежить від Kind

    [MaxLength(1000)]
    [Column("correct_answers")]
    public string? CorrectAnswers { get; set; }             // JSON, структура залежить від Kind

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
