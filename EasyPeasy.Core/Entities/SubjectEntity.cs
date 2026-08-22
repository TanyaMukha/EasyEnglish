using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;
using EasyPeasy.Core.Interfaces.Fields;

namespace EasyPeasy.Core.Entities;

/// <summary>
/// A top-level grouping for <see cref="CourseEntity"/> (e.g. "English", "German"). Optional —
/// a course does not have to belong to a subject (see <see cref="CourseEntity.SubjectId"/>).
/// </summary>
[Table("subjects")]
public class SubjectEntity : AbstractEntity, IAuditInfo
{
    [NotNull]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

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
}
