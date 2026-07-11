using EasyEnglish.Core.Entities;
using EasyEnglish.Data;

namespace EasyEnglish.Business.Tests.Fixtures;

/// <summary>Minimal entity builders for seeding a <see cref="EasyEnglishDbContext"/> in tests — every learnable entity requires a real parent chain (SQLite enforces FK constraints under EF Core).</summary>
public static class TestDataHelpers
{
    public static async Task<CourseEntity> SeedCourseAsync(EasyEnglishDbContext ctx, string title = "Course")
    {
        var course = new CourseEntity { Title = title };
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();
        return course;
    }

    public static async Task<UnitEntity> SeedUnitAsync(EasyEnglishDbContext ctx, int? courseId = null, string title = "Unit")
    {
        var actualCourseId = courseId ?? (await SeedCourseAsync(ctx)).Id;

        var unit = new UnitEntity { Title = title, CourseId = actualCourseId };
        ctx.Units.Add(unit);
        await ctx.SaveChangesAsync();
        return unit;
    }

    public static async Task<WordEntity> SeedWordAsync(
        EasyEnglishDbContext ctx, int unitId, string word = "word",
        float rate = 3f, DateTime? lastReviewDate = null, int reviewCount = 0)
    {
        var entity = new WordEntity
        {
            Word = word,
            UnitId = unitId,
            Rate = rate,
            LastReviewDate = lastReviewDate,
            ReviewCount = reviewCount,
        };
        ctx.Words.Add(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }

    public static async Task<ExampleEntity> SeedExampleAsync(EasyEnglishDbContext ctx, int wordId, string sentence = "sentence")
    {
        var entity = new ExampleEntity { Sentence = sentence, WordId = wordId };
        ctx.Examples.Add(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }

    public static async Task<IrregularFormEntity> SeedIrregularFormAsync(EasyEnglishDbContext ctx, int unitId, string firstForm = "go")
    {
        var entity = new IrregularFormEntity
        {
            FirstForm = firstForm,
            PartOfSpeech = "verb",
            SecondForm = "went",
            UnitId = unitId,
        };
        ctx.IrregularForms.Add(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }

    public static async Task<StudyCardEntity> SeedStudyCardAsync(EasyEnglishDbContext ctx, int unitId, string title = "card")
    {
        var entity = new StudyCardEntity { Title = title, UnitId = unitId };
        ctx.StudyCards.Add(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }

    public static async Task<TestCardEntity> SeedTestCardAsync(EasyEnglishDbContext ctx, int unitId, string title = "card")
    {
        var entity = new TestCardEntity { Title = title, UnitId = unitId };
        ctx.TestCards.Add(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }
}
