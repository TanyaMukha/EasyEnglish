using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyEnglish.Core.Entities;

[Table("study_card_items")]
public class StudyCardItemEntity
{
    [Key]
    [Column("card_id", Order = 0)]
    [ForeignKey("StudyCard")]
    public int CardId { get; set; }

    [Key]
    [Column("word_id", Order = 1)]
    [ForeignKey("Word")]
    public int WordId { get; set; }

    public virtual StudyCardEntity? StudyCard { get; set; }

    public virtual DictionaryItemEntity? Word { get; set; }
}
