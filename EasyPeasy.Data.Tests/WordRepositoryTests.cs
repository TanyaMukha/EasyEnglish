using EasyPeasy.Core.Options;
using EasyPeasy.Data.Repositories;
using EasyPeasy.Data.Tests.Fixtures;

namespace EasyPeasy.Data.Tests;

public class WordRepositoryTests : SqliteTestBase
{
    private WordRepository CreateRepository() => new(Factory, UserContext);

    [Fact]
    public async Task GetNextWordsAsync_OrdersByLastReviewDateAscending_NullsFirst()
    {
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewedRecently", lastReviewDate: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "neverReviewed");
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewedLongAgo", lastReviewDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        var result = await CreateRepository().GetNextWordsAsync(2);

        Assert.Equal(2, result.Count);
        Assert.Equal("neverReviewed", result[0].Word);
        Assert.Equal("reviewedLongAgo", result[1].Word);
    }

    [Fact]
    public async Task GetHardWordsAsync_OrdersByRateDescending()
    {
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "easy", rate: 1f);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "hardest", rate: 5f);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "medium", rate: 3f);
        }

        var result = await CreateRepository().GetHardWordsAsync(2);

        Assert.Equal(2, result.Count);
        Assert.Equal("hardest", result[0].Word);
        Assert.Equal("medium", result[1].Word);
    }

    [Fact]
    public async Task GetByUnitAsync_ReturnsOnlyWordsForThatUnit()
    {
        int unitAId;
        await using (var ctx = CreateContext())
        {
            var unitA = await TestDataHelpers.SeedUnitAsync(ctx);
            var unitB = await TestDataHelpers.SeedUnitAsync(ctx);
            unitAId = unitA.Id;
            await TestDataHelpers.SeedWordAsync(ctx, unitA.Id, "a1");
            await TestDataHelpers.SeedWordAsync(ctx, unitA.Id, "a2");
            await TestDataHelpers.SeedWordAsync(ctx, unitB.Id, "b1");
        }

        var result = await CreateRepository().GetByUnitAsync(unitAId);

        Assert.Equal(2, result.Count);
        Assert.All(result, w => Assert.Equal(unitAId, w.UnitId));
    }

    [Fact]
    public async Task GetByUnitAsync_WithIncludes_LoadsExamples()
    {
        int unitId;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            unitId = unit.Id;
            var seededWord = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word");
            await TestDataHelpers.SeedExampleAsync(ctx, seededWord.Id, "An example sentence.");
        }

        var result = await CreateRepository().GetByUnitAsync(unitId, includes: ["Examples"]);

        var word = Assert.Single(result);
        Assert.Single(word.Examples);
    }

    [Fact]
    public async Task CountReviewedSinceAsync_CountsOnlyWordsReviewedOnOrAfterCutoff()
    {
        var cutoff = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "before", lastReviewDate: cutoff.AddDays(-1));
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "onCutoff", lastReviewDate: cutoff);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "after", lastReviewDate: cutoff.AddDays(1));
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "neverReviewed");
        }

        var count = await CreateRepository().CountReviewedSinceAsync(cutoff);

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetForLearningAsync_ScopesToCourseAndOptionallyUnit()
    {
        int courseId, unitAId;
        await using (var ctx = CreateContext())
        {
            var course = await TestDataHelpers.SeedCourseAsync(ctx);
            courseId = course.Id;
            var unitA = await TestDataHelpers.SeedUnitAsync(ctx, course.Id);
            var unitB = await TestDataHelpers.SeedUnitAsync(ctx, course.Id);
            unitAId = unitA.Id;
            var otherCourseUnit = await TestDataHelpers.SeedUnitAsync(ctx);

            await TestDataHelpers.SeedWordAsync(ctx, unitA.Id, "a", rate: 3f);
            await TestDataHelpers.SeedWordAsync(ctx, unitB.Id, "b", rate: 3f);
            await TestDataHelpers.SeedWordAsync(ctx, otherCourseUnit.Id, "otherCourse", rate: 3f);
        }

        var options = new LearningSelectionOptions { IncludeLearnedWords = true, WordCount = 0 };

        var wholeCourse = await CreateRepository().GetForLearningAsync(courseId, unitId: null, options);
        var justUnitA = await CreateRepository().GetForLearningAsync(courseId, unitAId, options);

        Assert.Equal(2, wholeCourse.Count);
        Assert.DoesNotContain(wholeCourse, w => w.Word == "otherCourse");
        Assert.Equal(["a"], justUnitA.Select(w => w.Word));
    }
}
