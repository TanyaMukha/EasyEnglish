using EasyPeasy.Data.Repositories;
using EasyPeasy.Data.Tests.Fixtures;

namespace EasyPeasy.Data.Tests;

/// <summary>
/// Boundary tests for the cyclic-navigation logic duplicated across <see cref="WordRepository"/>,
/// <see cref="StudyCardRepository"/>, and <see cref="TestCardRepository"/>'s <c>GetNavigationIdsAsync</c>
/// (see EasyPeasy.Data/README.md Known Issues #4). Deliberately kept as separate per-repository test
/// classes, mirroring the production duplication, so a bug introduced into only one copy is caught by
/// its own test class rather than being masked by shared test code.
/// </summary>
public class WordRepositoryNavigationTests : SqliteTestBase
{
    private WordRepository CreateRepository() => new(Factory, UserContext);

    [Fact]
    public async Task MiddleItem_ReturnsPreviousAndNextNeighbors()
    {
        int[] ids;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            ids =
            [
                (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "a")).Id,
                (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "b")).Id,
                (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "c")).Id,
            ];
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), ids[1]);

        Assert.Equal(ids[0], previousId);
        Assert.Equal(ids[2], nextId);
        Assert.Equal(2, position);
        Assert.Equal(3, total);
    }

    [Fact]
    public async Task FirstItem_WrapsPreviousToLast()
    {
        int[] ids;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            ids =
            [
                (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "a")).Id,
                (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "b")).Id,
                (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "c")).Id,
            ];
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), ids[0]);

        Assert.Equal(ids[2], previousId);
        Assert.Equal(ids[1], nextId);
        Assert.Equal(1, position);
        Assert.Equal(3, total);
    }

    [Fact]
    public async Task LastItem_WrapsNextToFirst()
    {
        int[] ids;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            ids =
            [
                (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "a")).Id,
                (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "b")).Id,
                (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "c")).Id,
            ];
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), ids[2]);

        Assert.Equal(ids[1], previousId);
        Assert.Equal(ids[0], nextId);
        Assert.Equal(3, position);
        Assert.Equal(3, total);
    }

    [Fact]
    public async Task SingleItemUnit_HasNoNeighbors()
    {
        int id;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            id = (await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "only")).Id;
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), id);

        Assert.Null(previousId);
        Assert.Null(nextId);
        Assert.Equal(1, position);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task IdNotInUnit_ReturnsZeroPositionButRealTotal()
    {
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "a");
            await TestDataHelpers.SeedWordAsync(ctx, unit.Id, "b");
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), currentWordId: 999_999);

        Assert.Null(previousId);
        Assert.Null(nextId);
        Assert.Equal(0, position);
        Assert.Equal(2, total);
    }

    private async Task<int> GetUnitId()
    {
        await using var ctx = CreateContext();
        return ctx.Units.Single().Id;
    }
}

public class StudyCardRepositoryNavigationTests : SqliteTestBase
{
    private StudyCardRepository CreateRepository() => new(Factory, UserContext);

    [Fact]
    public async Task MiddleItem_ReturnsPreviousAndNextNeighbors()
    {
        int[] ids;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            ids =
            [
                (await TestDataHelpers.SeedStudyCardAsync(ctx, unit.Id, "a")).Id,
                (await TestDataHelpers.SeedStudyCardAsync(ctx, unit.Id, "b")).Id,
                (await TestDataHelpers.SeedStudyCardAsync(ctx, unit.Id, "c")).Id,
            ];
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), ids[1]);

        Assert.Equal(ids[0], previousId);
        Assert.Equal(ids[2], nextId);
        Assert.Equal(2, position);
        Assert.Equal(3, total);
    }

    [Fact]
    public async Task FirstItem_WrapsPreviousToLast()
    {
        int[] ids;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            ids =
            [
                (await TestDataHelpers.SeedStudyCardAsync(ctx, unit.Id, "a")).Id,
                (await TestDataHelpers.SeedStudyCardAsync(ctx, unit.Id, "b")).Id,
            ];
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), ids[0]);

        Assert.Equal(ids[1], previousId);
        Assert.Equal(ids[1], nextId);
        Assert.Equal(1, position);
        Assert.Equal(2, total);
    }

    [Fact]
    public async Task SingleItemUnit_HasNoNeighbors()
    {
        int id;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            id = (await TestDataHelpers.SeedStudyCardAsync(ctx, unit.Id, "only")).Id;
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), id);

        Assert.Null(previousId);
        Assert.Null(nextId);
        Assert.Equal(1, position);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task IdNotInUnit_ReturnsZeroPositionButRealTotal()
    {
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            await TestDataHelpers.SeedStudyCardAsync(ctx, unit.Id, "a");
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), currentCardId: 999_999);

        Assert.Null(previousId);
        Assert.Null(nextId);
        Assert.Equal(0, position);
        Assert.Equal(1, total);
    }

    private async Task<int> GetUnitId()
    {
        await using var ctx = CreateContext();
        return ctx.Units.Single().Id;
    }
}

public class TestCardRepositoryNavigationTests : SqliteTestBase
{
    private TestCardRepository CreateRepository() => new(Factory, UserContext);

    [Fact]
    public async Task LastItem_WrapsNextToFirst()
    {
        int[] ids;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            ids =
            [
                (await TestDataHelpers.SeedTestCardAsync(ctx, unit.Id, "a")).Id,
                (await TestDataHelpers.SeedTestCardAsync(ctx, unit.Id, "b")).Id,
                (await TestDataHelpers.SeedTestCardAsync(ctx, unit.Id, "c")).Id,
            ];
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), ids[2]);

        Assert.Equal(ids[1], previousId);
        Assert.Equal(ids[0], nextId);
        Assert.Equal(3, position);
        Assert.Equal(3, total);
    }

    [Fact]
    public async Task SingleItemUnit_HasNoNeighbors()
    {
        int id;
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            id = (await TestDataHelpers.SeedTestCardAsync(ctx, unit.Id, "only")).Id;
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), id);

        Assert.Null(previousId);
        Assert.Null(nextId);
        Assert.Equal(1, position);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task IdNotInUnit_ReturnsZeroPositionButRealTotal()
    {
        await using (var ctx = CreateContext())
        {
            var unit = await TestDataHelpers.SeedUnitAsync(ctx);
            await TestDataHelpers.SeedTestCardAsync(ctx, unit.Id, "a");
            await TestDataHelpers.SeedTestCardAsync(ctx, unit.Id, "b");
        }

        var (previousId, nextId, position, total) = await CreateRepository()
            .GetNavigationIdsAsync(await GetUnitId(), currentCardId: 999_999);

        Assert.Null(previousId);
        Assert.Null(nextId);
        Assert.Equal(0, position);
        Assert.Equal(2, total);
    }

    private async Task<int> GetUnitId()
    {
        await using var ctx = CreateContext();
        return ctx.Units.Single().Id;
    }
}
