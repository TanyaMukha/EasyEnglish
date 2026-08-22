using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;
using EasyPeasy.Core.Interfaces.Fields;

namespace EasyPeasy.Core.Entities;

/// <summary>
/// A learning course (e.g. "Business English"), optionally grouped under a <see cref="SubjectEntity"/>
/// and made up of one or more <see cref="UnitEntity"/> records.
/// </summary>
[Table("courses")]
public class CourseEntity : AbstractEntity, IGuidInfo, IAuditInfo, IGuidRecord
{
    /// <summary>
    /// Gets or sets the identifier of the entity.
    /// </summary>
    [Required]
    [Column("guid")]
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    [NotNull]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The course's language, as a BCP-47 tag (e.g. "en-us"). Used to pick which flag icon to
    /// display for the course.
    /// </summary>
    [MaxLength(20)]
    [Column("language_code")]
    public string? LanguageCode { get; set; } = "en-us";

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

    [Column("subject_id")]
    public int? SubjectId { get; set; }

    public SubjectEntity? Subject { get; set; }

    public List<UnitEntity> Units { get; set; } = new();
}
