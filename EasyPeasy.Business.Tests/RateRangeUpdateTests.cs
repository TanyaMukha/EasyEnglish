using EasyPeasy.Business.Tests.Fixtures;
using EasyPeasy.Core.Interfaces.Services;

namespace EasyPeasy.Business.Tests;

/// <summary>
/// <c>UpdateRateRangeAsync</c> on <see cref="EasyPeasy.Business.Services.IrregularFormService"/>,
/// <see cref="EasyPeasy.Business.Services.StudyCardService"/>, and
/// <see cref="EasyPeasy.Business.Services.TestCardService"/> share the same shape as
/// <see cref="EasyPeasy.Business.Services.WordService.UpdateWordRateRangeAsync"/> — updates matched
/// ids, silently skips ids that don't match an existing row.
/// </summary>
public class RateRangeUpdateTests : SqliteTestBase
{
    [Fact]
    public async Task IrregularFormService_UpdateRateRangeAsync_UpdatesMatched_SkipsUnmatched()
    {
        int formId;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            formId = (await TestDataHelpers.SeedIrregularFormAsync(ctx, unit.Id)).Id;
        }

        var results = await IrregularFormService.UpdateRateRangeAsync(
        [
            new UpdateWordRateRequest { Id = formId, Rate = 4.5f, ReviewCount = 3 },
            new UpdateWordRateRequest { Id = 999_999, Rate = 1f },
        ]);

        var result = Assert.Single(results);
        Assert.Equal(formId, result.Id);
        Assert.Equal(4.5f, result.Rate);
        Assert.Equal(3, result.ReviewCount);
    }

    [Fact]
    public async Task StudyCardService_UpdateRateRangeAsync_UpdatesMatched_SkipsUnmatched()
    {
        int cardId;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            cardId = (await TestDataHelpers.SeedStudyCardAsync(ctx, unit.Id)).Id;
        }

        var results = await StudyCardService.UpdateRateRangeAsync(
        [
            new UpdateWordRateRequest { Id = cardId, Rate = 2f },
            new UpdateWordRateRequest { Id = 999_999, Rate = 1f },
        ]);

        var result = Assert.Single(results);
        Assert.Equal(cardId, result.Id);
        Assert.Equal(2f, result.Rate);
    }

    [Fact]
    public async Task TestCardService_UpdateRateRangeAsync_UpdatesMatched_SkipsUnmatched()
    {
        int cardId;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            cardId = (await TestDataHelpers.SeedTestCardAsync(ctx, unit.Id)).Id;
        }

        var results = await TestCardService.UpdateRateRangeAsync(
        [
            new UpdateWordRateRequest { Id = cardId, Rate = 3.5f },
            new UpdateWordRateRequest { Id = 999_999, Rate = 1f },
        ]);

        var result = Assert.Single(results);
        Assert.Equal(cardId, result.Id);
        Assert.Equal(3.5f, result.Rate);
    }
}
