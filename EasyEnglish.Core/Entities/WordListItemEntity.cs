using System.ComponentModel.DataAnnotations.Schema;

namespace EasyEnglish.Core.Entities;

[Table("word_list_items")]
public class WordListItemEntity
{
    [ForeignKey("List")]
    [Column("list_id")]
    public int ListId { get; set; }

    [ForeignKey("Word")]
    [Column("word_id")]
    public int WordId { get; set; }

    public virtual WordListEntity? List { get; set; }

    public virtual DictionaryItemEntity? Word { get; set; }
}
