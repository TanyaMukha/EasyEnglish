using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyEnglish.Core.Entities;

[Table("word_tags")]
public class WordTagEntity
{
    [Key]
    [Column("word_id", Order = 0)]
    [ForeignKey("Word")]
    public int WordId { get; set; }

    [Key]
    [Column("tag_id", Order = 1)]
    [ForeignKey("Tag")]
    public int TagId { get; set; }

    public virtual DictionaryItemEntity? Word { get; set; }

    public virtual TagEntity? Tag { get; set; }
}
