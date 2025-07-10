using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLib.Database;
using EasyEnglish.Core.Interfaces;

namespace EasyEnglish.Core.Entities;

[Table("grammar_topics")]
public class GrammarTopicEntity : AbstractMainEntity, IReviewInfo, IAuditInfo
{
    [NotNull]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [NotNull]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [NotNull]
    [MaxLength(10)]
    [Column("language")]
    public string Language { get; set; } = "en";

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

    [ForeignKey("ParentTopic")]
    [Column("topic_id")]
    public int? TopicId { get; set; }

    public virtual GrammarTopicEntity? ParentTopic { get; set; }

    public virtual List<GrammarTopicEntity> ChildTopics { get; set; } = new();

    public virtual List<GrammarTopicTestEntity> GrammarTopicTests { get; set; } = new();
}
