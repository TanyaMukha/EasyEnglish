using EasyEnglish.Core.Enums;
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
        await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "atThreshold", rate: 1.6f);
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
    public async Task Old_UsesSameNeverReviewedFilterAsNew_JustAscendingOrder()
    {
        // Documents actual behavior, not necessarily intended behavior: LearningPriority.Old's own
        // XML doc says "not reviewed for the longest time" (which sounds like it should target
        // *reviewed-but-overdue* items, similar to Review), but the implementation filters to
        // LastReviewDate == null -- identical to New's filter -- differing only in sort direction.
        // See EasyEnglish.Data/README.md Known Issues #5.
        await using var ctx = CreateContext();
        var unit = await TestDataHelpers.SeedUnitAsync(ctx);
        var reviewed = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "reviewed", lastReviewDate: DateTime.UtcNow);
        var older = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "olderWord");
        var newer = await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "newerWord");
        await TestDataHelpers.SetWordCreatedAtAsync(ctx, older.Id, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        await TestDataHelpers.SetWordCreatedAtAsync(ctx, newer.Id, new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await ctx.Words.AsNoTracking().ApplyLearningSelectionAsync(
            new LearningSelectionOptions { Priority = LearningPriority.Old, WordCount = 0, IncludeLearnedWords = true });

        Assert.DoesNotContain(result, w => w.Id == reviewed.Id);
        Assert.Equal(["olderWord", "newerWord"], result.Select(w => w.Word));
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
