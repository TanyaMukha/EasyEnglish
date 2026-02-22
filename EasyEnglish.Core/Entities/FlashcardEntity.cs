using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EasyEnglish.Core.Entities
{
    internal class FlashcardEntity : AbstractEntity, IReviewInfo, IRateInfo, IAuditInfo
    {
        [NotNull]
        [MaxLength(500)]
        [Column("front")]
        public string Front { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Column("back")]
        public string Back { get; set; } = string.Empty;

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

        [ForeignKey("Unit")]
        [Column("unit_id")]
        public int UnitId { get; set; }

        public UnitEntity? Unit { get; set; }
    }
}
