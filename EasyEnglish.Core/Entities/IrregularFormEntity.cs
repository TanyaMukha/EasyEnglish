using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyEnglish.Core.Entities;

[Table("irregular_forms")]
public class IrregularFormEntity
{
    [Key]
    [Column("first_form_id", Order = 0)]
    [ForeignKey("FirstForm")]
    public int FirstFormId { get; set; }

    [Key]
    [Column("second_form_id", Order = 1)]
    [ForeignKey("SecondForm")]
    public int SecondFormId { get; set; }

    [Key]
    [Column("third_form_id", Order = 2)]
    [ForeignKey("ThirdForm")]
    public int? ThirdFormId { get; set; }

    public virtual DictionaryItemEntity? FirstForm { get; set; }

    public virtual DictionaryItemEntity? SecondForm { get; set; }

    public virtual DictionaryItemEntity? ThirdForm { get; set; }
}
