using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLib.Database;
using EasyEnglish.Core.Interfaces;

namespace EasyEnglish.Core.Entities;

[Table("dictionary_items")]
public class DictionaryItemEntity : AbstractMainEntity, IReviewInfo, IRateInfo, IAuditInfo
{
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

    [MaxLength(500)]
    [Column("explanation")]
    public string? Explanation { get; set; }

    [MaxLength(500)]
    [Column("definition")]
    public string? Definition { get; set; }

    [NotNull]
    [MaxLength(50)]
    [Column("part_of_speech")]
    public string PartOfSpeech { get; set; } = string.Empty;

    [NotNull]
    [MaxLength(10)]
    [Column("language")]
    public string Language { get; set; } = "en";

    [NotNull]
    [MaxLength(10)]
    [Column("level")]
    public string Level { get; set; } = "A1";

    [Column("is_irregular")]
    public bool IsIrregular { get; set; } = false;

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

    [ForeignKey("Dictionary")]
    [Column("dictionary_id")]
    public int DictionaryId { get; set; }

    public virtual DictionaryEntity? Dictionary { get; set; }

    public virtual List<ExampleEntity> Examples { get; set; } = new();

    public virtual List<WordTagEntity> WordTags { get; set; } = new();

    public virtual List<WordListItemEntity> WordLists { get; set; } = new();
}
