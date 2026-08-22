using EasyPeasy.Business.Tests.Fixtures;
using EasyPeasy.Core.Enums;
using EasyPeasy.Core.Options;

namespace EasyPeasy.Business.Tests;

public class CourseServiceTests : SqliteTestBase
{
    [Fact]
    public async Task GetWordsAsync_ShuffleFalse_PreservesRepositoryOrder()
    {
        int courseId;
        await using (var ctx = CreateContext())
        {
            var course = await TestDataHelpers.SeedCourseAsync(ctx);
            courseId = course.Id;
            var unit = await TestDataHelpers.SeedUnitAsync(ctx, course.Id);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "low", rate: 1f);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "high", rate: 5f);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "mid", rate: 3f);
        }

        var options = new LearningSelectionOptions
        {
            Priority = LearningPriority.Difficult,
            ShuffleWords = false,
            IncludeLearnedWords = true,
            WordCount = 0,
        };

        var words = await CourseService.GetWordsAsync(courseId, unitId: null, options);

        Assert.Equal(["high", "mid", "low"], words.Select(w => w.Word));
    }

    [Fact]
    public async Task GetWordsAsync_ShuffleTrue_PreservesMembershipAndCount()
    {
        int courseId;
        await using (var ctx = CreateContext())
        {
            var course = await TestDataHelpers.SeedCourseAsync(ctx);
            courseId = course.Id;
            var unit = await TestDataHelpers.SeedUnitAsync(ctx, course.Id);
            for (var i = 0; i < 5; i++)
                await TestDataHelpers.SeedWordAsync(ctx, unit.Id, $"word{i}", rate: 3f);
        }

        var options = new LearningSelectionOptions
        {
            Priority = LearningPriority.Difficult,
            ShuffleWords = true,
            IncludeLearnedWords = true,
            WordCount = 0,
        };

        var words = (await CourseService.GetWordsAsync(courseId, unitId: null, options)).ToList();

        Assert.Equal(5, words.Count);
        Assert.Equal(Enumerable.Range(0, 5).Select(i => $"word{i}").OrderBy(w => w), words.Select(w => w.Word).OrderBy(w => w));
    }

    [Fact]
    public async Task GetUnitsAsync_ReturnsUnitsForCourse()
    {
        int courseId;
        await using (var ctx = CreateContext())
        {
            var course = await TestDataHelpers.SeedCourseAsync(ctx);
            courseId = course.Id;
            await TestDataHelpers.SeedUnitAsync(ctx, course.Id, "Unit A");
            await TestDataHelpers.SeedUnitAsync(ctx, course.Id, "Unit B");
        }

        var units = await CourseService.GetUnitsAsync(courseId);

        Assert.Equal(2, units.Count());
    }
}
