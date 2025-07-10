using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyEnglish.Core.Entities;

[Table("grammar_topic_tests")]
public class GrammarTopicTestEntity
{
    [Key]
    [Column("topic_id", Order = 0)]
    [ForeignKey("GrammarTopic")]
    public int TopicId { get; set; }

    [Key]
    [Column("test_id", Order = 1)]
    [ForeignKey("GrammarTest")]
    public int TestId { get; set; }

    public virtual GrammarTopicEntity? GrammarTopic { get; set; }

    public virtual GrammarTestEntity? GrammarTest { get; set; }
}
