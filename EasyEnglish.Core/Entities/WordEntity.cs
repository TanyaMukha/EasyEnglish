using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;
using EasyEnglish.Core.Interfaces.Fields;

namespace EasyEnglish.Core.Entities;

[Table("words")]
public class WordEntity : AbstractEntity, IReviewInfo, IRateInfo, IAuditInfo, IGuidRecord
{
    [Column("guid")]
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    [NotNull]
    [MaxLength(100)]
    [Column("word")]
    public string Word { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("transcription")]
    public string? Transcription { get; set; }

    [MaxLength(300)]
    [Column("translation")]
    public string? Translation { get; set; }

    /// <summary>
    /// Додаткова примітка до слова (найчастіше позначається у дужках,
    /// наприклад форми неправильного дієслова "get-got-got").
    /// Не показується в режимі навчання.
    /// </summary>
    [MaxLength(300)]
    [Column("note")]
    public string? Note { get; set; }

    [Column("pronunciation")]
    public byte[]? Pronunciation { get; set; }

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

    public List<ExampleEntity> Examples { get; set; } = new();
}
