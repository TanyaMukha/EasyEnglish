using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Extensions;
using EasyEnglish.Core.Options;
using EasyEnglish.Data.Extensions;
using EasyEnglish.Data.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace EasyEnglish.Data.Tests;

/// <summary>
/// Tests for <see cref="LearningQueryExtensions.ApplyLearningSelectionAsync{T}"/> against a real
/// SQLite database — the whole point is confirming the <c>EF.Property</c> calls actually translate to
/// SQL, not just that the equivalent LINQ-to-Objects logic would be correct.
/// </summary>
public class LearningQueryExtensionsTests : SqliteTestBase
{
    [Fact]
    public async Task IncludeLearnedWordsFalse_ExcludesRatesBelowThreshold()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "easy", rate: 1.0f);
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "atThreshold", rate: RateExtensions.EasyMax);
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "hard", rate: 4.0f);

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { IncludeLearnedWords = false, WordCount = 0 });

        Assert.DoesNotContain(result, w => w.Word == "easy");
        Assert.Contains(result, w => w.Word == "atThreshold");
        Assert.Contains(result, w => w.Word == "hard");
    }

    [Fact]
    public async Task IncludeLearnedWordsTrue_IncludesEverything()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "easy", rate: 1.0f);
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "hard", rate: 4.0f);

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { IncludeLearnedWords = true, WordCount = 0 });

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Random_WordCountZero_ReturnsAllMatchingWords()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        for (var i = 0; i < 5; i++)
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, $"word{i}", rate: 3f);

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.Random, WordCount = 0, IncludeLearnedWords = true });

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task Random_WordCountLimits_ReturnsExactlyThatManyDistinctWords()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        for (var i = 0; i < 10; i++)
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, $"word{i}", rate: 3f);

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.Random, WordCount = 4, IncludeLearnedWords = true });

        Assert.Equal(4, result.Count);
        Assert.Equal(4, result.Select(w => w.Id).Distinct().Count());
    }

    [Fact]
    public async Task Difficult_OrdersByRateDescending()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "low", rate: 2f);
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "high", rate: 5f);
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "mid", rate: 3.5f);

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.Difficult, WordCount = 0, IncludeLearnedWords = true });

        Assert.Equal(["high", "mid", "low"], result.Select(w => w.Word));
    }

    [Fact]
    public async Task New_OnlyIncludesNeverReviewedWords_OrderedByCreatedAtDescending()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        var reviewed = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewed", lastReviewDate: DateTime.UtcNow);
        var older = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "olderNew");
        var newer = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "newerNew");
        await TestDataHelpers.SetWordCreatedAtAsync(ctx, older.Id, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        await TestDataHelpers.SetWordCreatedAtAsync(ctx, newer.Id, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.New, WordCount = 0, IncludeLearnedWords = true });

        Assert.DoesNotContain(result, w => w.Id == reviewed.Id);
        Assert.Equal(["newerNew", "olderNew"], result.Select(w => w.Word));
    }

    [Fact]
    public async Task Review_OnlyIncludesReviewedWords_OrderedByLastReviewDateAscending()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        var neverReviewed = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "neverReviewed");
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewedRecently", lastReviewDate: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewedLongAgo", lastReviewDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.Review, WordCount = 0, IncludeLearnedWords = true });

        Assert.DoesNotContain(result, w => w.Id == neverReviewed.Id);
        Assert.Equal(["reviewedLongAgo", "reviewedRecently"], result.Select(w => w.Word));
    }

    [Fact]
    public async Task Recent_ReturnsReviewedItemsNewestFirst_AndSkipsNeverReviewed()
    {
        // "Recent" — what the learner worked on last. Mirror of Old, but never-reviewed items
        // are dropped entirely: they were never studied, so they can't be "studied before this".
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewedLongAgo",
            lastReviewDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewedYesterday",
            lastReviewDate: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        var neverReviewed = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "neverReviewed");

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.Recent, WordCount = 0, IncludeLearnedWords = true });

        Assert.DoesNotContain(result, w => w.Id == neverReviewed.Id);
        Assert.Equal(["reviewedYesterday", "reviewedLongAgo"], result.Select(w => w.Word));
    }

    [Fact]
    public async Task Recent_WordCountTakesTheMostRecentOnes()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        for (var i = 1; i <= 5; i++)
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, $"day{i}",
                lastReviewDate: new DateTime(2024, 1, i, 0, 0, 0, DateTimeKind.Utc));

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.Recent, WordCount = 2, IncludeLearnedWords = true });

        Assert.Equal(["day5", "day4"], result.Select(w => w.Word));
    }

    [Fact]
    public async Task Old_SpansBothReviewedAndNeverReviewedItems_OrderedByLastTouchAscending()
    {
        // LearningPriority.Old is a broader "staleness" ranking than New (never-reviewed only) or
        // Review (reviewed only): it spans BOTH pools, ranking each item by whichever timestamp
        // reflects when it was last touched -- LastReviewDate if reviewed, CreatedAt otherwise --
        // so a long-neglected never-reviewed item and a long-ago-reviewed item both count as "old".
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        var reviewedLongAgo = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewedLongAgo",
            lastReviewDate: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var reviewedRecently = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewedRecently",
            lastReviewDate: new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        var neverReviewedOld = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "neverReviewedOld");
        var neverReviewedNew = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "neverReviewedNew");
        await TestDataHelpers.SetWordCreatedAtAsync(ctx, neverReviewedOld.Id, new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        await TestDataHelpers.SetWordCreatedAtAsync(ctx, neverReviewedNew.Id, new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.Old, WordCount = 0, IncludeLearnedWords = true });

        Assert.Equal(
            ["neverReviewedOld", "reviewedLongAgo", "reviewedRecently", "neverReviewedNew"],
            result.Select(w => w.Word));
    }

    [Fact]
    public async Task WordCount_LimitsNonRandomPriorityToo()
    {
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        for (var i = 0; i < 5; i++)
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, $"word{i}", rate: i);

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.Difficult, WordCount = 2, IncludeLearnedWords = true });

        Assert.Equal(2, result.Count);
    }
}
