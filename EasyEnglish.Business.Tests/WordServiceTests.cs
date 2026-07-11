using EasyEnglish.Business.Tests.Fixtures;
using EasyEnglish.Core.Interfaces.Services;
using MukhaLab.Database;

namespace EasyEnglish.Business.Tests;

public class WordServiceTests : SqliteTestBase
{
    [Fact]
    public async Task UpdateWordRateAsync_ExistingWord_UpdatesRateAndReview()
    {
        int wordId;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            wordId = (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word", rate: 3f)).Id;
        }

        var reviewDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = await WordService.UpdateWordRateAsync(new UpdateWordRateRequest
        {
            Id = wordId,
            Rate = 4.5f,
            LastReviewDate = reviewDate,
            ReviewCount = 2,
        });

        Assert.Equal(4.5f, result.Rate);
        Assert.Equal(reviewDate, result.LastReviewDate);
        Assert.Equal(2, result.ReviewCount);
    }

    [Fact]
    public async Task UpdateWordRateAsync_WordNotFound_ThrowsEntityNotFoundException()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            WordService.UpdateWordRateAsync(new UpdateWordRateRequest { Id = 999_999, Rate = 4f }));
    }

    [Fact]
    public async Task UpdateWordRateRangeAsync_UpdatesMatchedWords_SkipsUnmatchedIds()
    {
        int word1Id, word2Id;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            word1Id = (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word1", rate: 3f)).Id;
            word2Id = (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word2", rate: 3f)).Id;
        }

        var results = await WordService.UpdateWordRateRangeAsync(
        [
            new UpdateWordRateRequest { Id = word1Id, Rate = 5f },
            new UpdateWordRateRequest { Id = word2Id, Rate = 1f },
            new UpdateWordRateRequest { Id = 999_999, Rate = 2f },
        ]);

        Assert.Equal(2, results.Count());
        Assert.Contains(results, w => w.Id == word1Id && w.Rate == 5f);
        Assert.Contains(results, w => w.Id == word2Id && w.Rate == 1f);
    }

    [Fact]
    public async Task GetNavigationIdsAsync_DelegatesToRepository()
    {
        int unitId, word1Id, word2Id;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            unitId = unit.Id;
            word1Id = (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word1")).Id;
            word2Id = (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "word2")).Id;
        }

        var (previousId, nextId, position, total) = await WordService.GetNavigationIdsAsync(unitId, word1Id);

        Assert.Equal(word2Id, previousId);
        Assert.Equal(word2Id, nextId);
        Assert.Equal(1, position);
        Assert.Equal(2, total);
    }
}
