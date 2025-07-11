using EasyEnglish.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyEnglish.Data;

public class EasyEnglishDbContext : DbContext
{
    public EasyEnglishDbContext(DbContextOptions<EasyEnglishDbContext> options)
        : base(options)
    {
    }

    public DbSet<DictionaryEntity> Dictionaries => this.Set<DictionaryEntity>();

    public DbSet<DictionaryItemEntity> DictionaryItems => this.Set<DictionaryItemEntity>();

    public DbSet<ExampleEntity> Examples => this.Set<ExampleEntity>();

    public DbSet<GrammarTestEntity> GrammarTests => this.Set<GrammarTestEntity>();

    public DbSet<GrammarTopicEntity> GrammarTopics => this.Set<GrammarTopicEntity>();

    public DbSet<GrammarTopicTestEntity> GrammarTopicTests => this.Set<GrammarTopicTestEntity>();

    public DbSet<IrregularFormEntity> IrregularForms => this.Set<IrregularFormEntity>();

    public DbSet<StudyCardEntity> StudyCards => this.Set<StudyCardEntity>();

    public DbSet<StudyCardItemEntity> StudyCardItems => this.Set<StudyCardItemEntity>();

    public DbSet<TagEntity> Tags => this.Set<TagEntity>();

    public DbSet<TestCardEntity> TestCards => this.Set<TestCardEntity>();

    public DbSet<UnitEntity> Units => this.Set<UnitEntity>();

    public DbSet<WordListEntity> WordLists => this.Set<WordListEntity>();

    public DbSet<WordListItemEntity> WordListItems => this.Set<WordListItemEntity>();

    public DbSet<WordTagEntity> WordTags => this.Set<WordTagEntity>();
}
