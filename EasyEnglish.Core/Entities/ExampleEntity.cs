using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MukhaLab.Database;

namespace EasyEnglish.Core.Entities;

[Table("examples")]
public class ExampleEntity : AbstractEntity
{
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
