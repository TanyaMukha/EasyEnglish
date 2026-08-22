using EasyPeasy.Cache.Services;
using EasyPeasy.Cache.Tests.Fixtures;
using EasyPeasy.Core.Interfaces.Services;
using EasyPeasy.Core.Models;
using NSubstitute;

namespace EasyPeasy.Cache.Tests;

/// <summary>
/// Tests for <see cref="CurrentUnitCacheService"/>, exercising
/// <see cref="BaseSingleCacheService{TEntity, TId}"/>'s shared caching logic through its concrete usage.
/// </summary>
public class CurrentUnitCacheServiceTests
{
    private readonly FakeStorageService _storage = new();
    private readonly IUnitService _unitService = Substitute.For<IUnitService>();

    private CurrentUnitCacheService CreateSut() => new(_storage, TestScopeFactory.ForInstance(_unitService));

    private static UnitModel Unit(int id) => new() { Id = id, Title = $"unit{id}" };

    private void SeedUnitServiceReturns(UnitModel unit)
    {
        _unitService.GetByIdAsync(unit.Id, Arg.Any<string[]?>(), Arg.Any<CancellationToken>())
            .Returns(unit);
    }

    [Fact]
    public async Task GetAsync_NoSelection_ReturnsNull()
    {
        var sut = CreateSut();

        Assert.Null(await sut.GetAsync());
    }

    [Fact]
    public async Task HasValueAsync_NoSelection_ReturnsFalse()
    {
        var sut = CreateSut();

        Assert.False(await sut.HasValueAsync());
    }

    [Fact]
    public async Task InitializeAsync_LoadsPersistedSelectionAndFetchesEntity()
    {
        await _storage.SetAsync("currentUnitId", 7);
        SeedUnitServiceReturns(Unit(7));
        var sut = CreateSut();

        var result = await sut.GetAsync();

        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
    }

    [Fact]
    public async Task InitializeAsync_CalledImplicitlyTwice_OnlyFetchesOnce()
    {
        await _storage.SetAsync("currentUnitId", 7);
        SeedUnitServiceReturns(Unit(7));
        var sut = CreateSut();

        await sut.GetAsync();
        await sut.GetAsync();

        await _unitService.Received(1).GetByIdAsync(7, Arg.Any<string[]?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAsync_NewId_FetchesCachesAndPersistsSelection()
    {
        SeedUnitServiceReturns(Unit(3));
        var sut = CreateSut();

        await sut.SetAsync(3);

        Assert.Equal(3, await sut.GetIdAsync());
        var cached = await sut.GetAsync();
        Assert.Equal(3, cached!.Id);
        Assert.Equal(3, await _storage.GetAsync<int>("currentUnitId"));
    }

    [Fact]
    public async Task SetAsync_SameIdAgain_StillRefetches()
    {
        // Re-selecting the same id must refresh the cached entity (closes the staleness gap from
        // the library's README Known Issues) instead of silently keeping a stale copy.
        SeedUnitServiceReturns(Unit(3));
        var sut = CreateSut();
        await sut.SetAsync(3); // first fetch

        await sut.SetAsync(3);

        await _unitService.Received(2).GetByIdAsync(3, Arg.Any<string[]?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAsync_DifferentId_RefetchesAndReplacesSelection()
    {
        SeedUnitServiceReturns(Unit(3));
        SeedUnitServiceReturns(Unit(4));
        var sut = CreateSut();
        await sut.SetAsync(3);

        await sut.SetAsync(4);

        Assert.Equal(4, await sut.GetIdAsync());
        var cached = await sut.GetAsync();
        Assert.Equal(4, cached!.Id);
    }

    [Fact]
    public async Task ClearAsync_DeselectsInMemoryAndInStorage()
    {
        SeedUnitServiceReturns(Unit(3));
        var sut = CreateSut();
        await sut.SetAsync(3);

        await sut.ClearAsync();

        Assert.False(await sut.HasValueAsync());
        Assert.Null(await sut.GetAsync());
    }

    [Fact]
    public async Task ClearCache_ResetsInMemoryState_ButRereadsUnchangedPersistedStorage()
    {
        await _storage.SetAsync("currentUnitId", 7);
        SeedUnitServiceReturns(Unit(7));
        var sut = CreateSut();
        await sut.GetAsync();

        sut.ClearCache();
        var result = await sut.GetAsync();

        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        await _unitService.Received(2).GetByIdAsync(7, Arg.Any<string[]?>(), Arg.Any<CancellationToken>());
    }
}
