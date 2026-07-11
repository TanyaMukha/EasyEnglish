using EasyEnglish.Core.Entities;
using EasyEnglish.Data.Repositories;
using EasyEnglish.Data.Tests.Fixtures;

namespace EasyEnglish.Data.Tests;

public class UnitRepositoryTests : SqliteTestBase
{
    private UnitRepository CreateRepository() => new(Factory, UserContext);

    [Fact]
    public async Task GetUnitCardsAsync_BucketsWordsAndIrregularFormsByDifficulty()
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
            await ctx.SaveChangesAsync();
        }

        await using var readCtx = CreateContext();
        var courseId = readCtx.Units.Single().CourseId;
        var repository = CreateRepository();

        var cards = await repository.GetUnitCardsAsync(courseId);

        var card = Assert.Single(cards);
        Assert.Equal("Unit 1", card.Title);
        Assert.Equal(4, card.TotalCount);
        Assert.Equal(2, card.EasyCount);
        Assert.Equal(1, card.MediumCount);
        Assert.Equal(1, card.HardCount);
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
