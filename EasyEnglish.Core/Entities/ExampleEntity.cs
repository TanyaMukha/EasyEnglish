using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;

namespace EasyEnglish.Core.Entities;

/// <summary>An example sentence demonstrating the usage of a <see cref="WordEntity"/>.</summary>
[Table("examples")]
public class ExampleEntity : AbstractEntity, IGuidRecord
{
    [Column("guid")]
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    [NotNull]
    [MaxLength(1000)]
    [Column("sentence")]
    public string Sentence { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Column("translation")]
    public string? Translation { get; set; }

    [ForeignKey("Word")]
    [Column("word_id")]
    public int WordId { get; set; }

    public WordEntity? Word { get; set; }
}
