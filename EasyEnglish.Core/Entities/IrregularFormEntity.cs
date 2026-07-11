using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace EasyEnglish.Core.Entities;

/// <summary>
/// An irregular verb (or other irregular word) belonging to a <see cref="UnitEntity"/>, tracked as
/// up to three forms (e.g. "go" / "went" / "gone") each with its own transcription/translation/
/// pronunciation, plus its own review/rating state independent of <see cref="WordEntity"/>.
/// </summary>
[Table("irregular_forms")]
public class IrregularFormEntity : AbstractEntity, IReviewInfo, IRateInfo, IAuditInfo, IGuidRecord
{
    [Column("guid")]
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    [NotNull]
    [MaxLength(100)]
    [Column("first_form")]
    public string FirstForm { get; set; } = string.Empty;

    [NotNull]
    [MaxLength(50)]
    [Column("part_of_speech")]
    public string? PartOfSpeech { get; set; }

    [MaxLength(100)]
    [Column("first_form_transcription")]
    public string? FirstFormTranscription { get; set; }

    [MaxLength(300)]
    [Column("first_form_translation")]
    public string? FirstFormTranslation { get; set; }

    [Column("first_form_pronunciation")]
    public byte[]? FirstFormPronunciation { get; set; }

    [NotNull]
    [MaxLength(100)]
    [Column("second_form")]
    public string SecondForm { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("second_form_transcription")]
    public string? SecondFormTranscription { get; set; }

    [MaxLength(300)]
    [Column("second_form_translation")]
    public string? SecondFormTranslation { get; set; }

    [Column("second_form_pronunciation")]
    public byte[]? SecondFormPronunciation { get; set; }

    [MaxLength(100)]
    [Column("third_form")]
    public string? ThirdForm { get; set; }

    [MaxLength(100)]
    [Column("third_form_transcription")]
    public string? ThirdFormTranscription { get; set; }

    [MaxLength(300)]
    [Column("third_form_translation")]
    public string? ThirdFormTranslation { get; set; }

    [Column("third_form_pronunciation")]
    public byte[]? ThirdFormPronunciation { get; set; }

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