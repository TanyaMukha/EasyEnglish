using EasyEnglish.Core.Entities;
using EasyEnglish.Data.Repositories;
using EasyEnglish.Data.Tests.Fixtures;

namespace EasyEnglish.Data.Tests;

public class UnitRepositoryTests : SqliteTestBase
{
    private UnitRepository CreateRepository() => new(Factory, UserContext);

    [Fact]
    public async Task GetUnitCardsAsync_BucketsEveryContentKindByDifficulty()
    {
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx, title: "Unit 1");

            // Easy: rate < EasyMax (5/3 ~ 1.667). Medium: [EasyMax, HardMin). Hard: >= HardMin (10/3 ~ 3.333).
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "easyWord", rate: 1.0f);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "mediumWord", rate: 2.5f);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "hardWord", rate: 4.0f);

            ctx.IrregularForms.Add(new IrregularFormEntity
            {
                FirstForm = "go",
                PartOfSpeech = "verb",
                SecondForm = "went",
                UnitId = unit.Id,
                Rate = 1.0f,
            });
            ctx.StudyCards.Add(new StudyCardEntity { Title = "study", UnitId = unit.Id, Rate = 2.0f });
            ctx.TestCards.Add(new TestCardEntity { Title = "test easy", UnitId = unit.Id, Rate = 0.5f });
            ctx.TestCards.Add(new TestCardEntity { Title = "test hard", UnitId = unit.Id, Rate = 5.0f });
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = CreateContext();
        var courseId = readCtx.Units.Single().CourseId;
        var repository = CreateRepository();

        var cards = await repository.GetUnitCardsAsync(courseId);

        var card = Assert.Single(cards);
        Assert.Equal("Unit 1", card.Title);

        Assert.Equal(3, card.WordCount);
        Assert.Equal(1, card.IrregularFormCount);
        Assert.Equal(1, card.StudyCardCount);
        Assert.Equal(2, card.TestCardCount);
        Assert.Equal(7, card.TotalCount);

        Assert.Equal(3, card.EasyCount);   // easyWord, irregular form, easy test card
        Assert.Equal(2, card.MediumCount); // mediumWord, study card
        Assert.Equal(2, card.HardCount);   // hardWord, hard test card
        Assert.Equal(card.TotalCount, card.EasyCount + card.MediumCount + card.HardCount);
    }

    [Fact]
    public async Task GetUnitCardsAsync_UnitWithOnlyTestCards_IsNotReportedAsEmpty()
    {
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx, title: "Prepositions");
            ctx.TestCards.Add(new TestCardEntity { Title = "card 1", UnitId = unit.Id, Rate = 3f });
            ctx.TestCards.Add(new TestCardEntity { Title = "card 2", UnitId = unit.Id, Rate = 1f });
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = CreateContext();
        var courseId = readCtx.Units.Single().CourseId;

        var card = Assert.Single(await CreateRepository().GetUnitCardsAsync(courseId));

        Assert.Equal(0, card.WordCount);
        Assert.Equal(2, card.TestCardCount);
        Assert.Equal(2, card.TotalCount);
        Assert.Equal(1, card.EasyCount);
        Assert.Equal(1, card.MediumCount);
    }

    [Fact]
    public async Task GetUnitCardsAsync_EasyMaxBoundary_CountsAsMedium()
    {
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "boundary", rate: EasyEnglish.Core.Extensions.RateExtensions.EasyMax);
        }

        await using var readCtx = CreateContext();
        var courseId = readCtx.Units.Single().CourseId;

        var cards = await CreateRepository().GetUnitCardsAsync(courseId);

        var card = Assert.Single(cards);
        Assert.Equal(0, card.EasyCount);
        Assert.Equal(1, card.MediumCount);
        Assert.Equal(0, card.HardCount);
    }

    [Fact]
    public async Task GetByCourseAsync_ReturnsOnlyUnitsForThatCourse()
    {
        await using (var ctx = CreateContext())
        {
            var courseA = await TestDataHelpers.SeedCourseAsync(ctx, "Course A");
            var courseB = await TestDataHelpers.SeedCourseAsync(ctx, "Course B");
            await TestDataHelpers.SeedUnitAsync(ctx, courseA.Id, "A1");
            await TestDataHelpers.SeedUnitAsync(ctx, courseA.Id, "A2");
            await TestDataHelpers.SeedUnitAsync(ctx, courseB.Id, "B1");
        }

        await using var readCtx = CreateContext();
        var courseAId = readCtx.Courses.Single(c => c.Title == "Course A").Id;

        var units = await CreateRepository().GetByCourseAsync(courseAId);

        Assert.Equal(2, units.Count);
        Assert.All(units, u => Assert.Equal(courseAId, u.CourseId));
    }
}
