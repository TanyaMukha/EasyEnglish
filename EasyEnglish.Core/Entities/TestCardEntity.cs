using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLib.Database;
using EasyEnglish.Core.Interfaces;

namespace EasyEnglish.Core.Entities;

[Table("test_cards")]
public class TestCardEntity : AbstractMainEntity, IReviewInfo, IRateInfo, IAuditInfo
{
    [MaxLength(50)]
    [Column("test_type")]
    public string? TestType { get; set; }

    [NotNull]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [MaxLength(2000)]
    [Column("text")]
    public string? Text { get; set; }

    [MaxLength(500)]
    [Column("mask")]
    public string? Mask { get; set; }

    [MaxLength(1000)]
    [Column("options")]
    public string? Options { get; set; }

    [MaxLength(500)]
    [Column("correct_answers")]
    public string? CorrectAnswers { get; set; }

    [Column("last_review_date")]
    public DateTime? LastReviewDate { get; set; }

    [Column("review_count")]
    public int ReviewCount { get; set; } = 0;

    [Column("rate")]
    public float Rate { get; set; } = 0;

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

    [ForeignKey("GrammarTest")]
    [Column("test_id")]
    public int TestId { get; set; }

    public virtual GrammarTestEntity? GrammarTest { get; set; }
}
