using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;
using EasyEnglish.Core.Interfaces.Fields;

namespace EasyEnglish.Core.Entities;

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
    /// Мова курсу у форматі BCP-47 (наприклад, "en-us"). Використовується для
    /// відображення прапора курсу.
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

    public List<UnitEntity> Units { get; set; } = new();
}
