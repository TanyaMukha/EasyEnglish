using EasyPeasy.Cache.Services;
using EasyPeasy.Cache.Tests.Fixtures;
using EasyPeasy.Core.Interfaces.Services;
using EasyPeasy.Core.Models;
using NSubstitute;

namespace EasyPeasy.Cache.Tests;

/// <summary>
/// Tests for <see cref="WordCacheService"/>, exercising <see cref="BaseCacheService{TEntity, TId}"/>'s
/// shared caching logic through its concrete usage.
/// </summary>
public class WordCacheServiceTests
{
    private readonly FakeStorageService _storage = new();
    private readonly IWordService _wordService = Substitute.For<IWordService>();

    private WordCacheService CreateSut() => new(_storage, TestScopeFactory.ForInstance(_wordService));

    private static WordModel Word(int id) => new() { Id = id, Word = $"word{id}" };

    private void SeedWordServiceReturns(params WordModel[] words)
    {
        _wordService.GetByIdsAsync(Arg.Any<IEnumerable<int>>(), Arg.Any<string[]?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var requestedIds = ((IEnumerable<int>)callInfo[0]).ToHashSet();
                return words.Where(w => requestedIds.Contains(w.Id)).ToList();
            });
    }

    [Fact]
    public async Task GetAllAsync_EmptyStorage_ReturnsEmpty()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task InitializeAsync_LoadsPersistedSelectionAndFetchesEntities()
    {
        await _storage.SetAsync("selectedWordIds", new List<int> { 1, 2 });
        SeedWordServiceReturns(Word(1), Word(2));
        var sut = CreateSut();

        var result = await sut.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, w => w.Id == 1);
        Assert.Contains(result, w => w.Id == 2);
    }

    [Fact]
    public async Task InitializeAsync_CalledImplicitlyTwice_OnlyFetchesOnce()
    {
        await _storage.SetAsync("selectedWordIds", new List<int> { 1 });
        SeedWordServiceReturns(Word(1));
        var sut = CreateSut();

        await sut.GetAllAsync();
        await sut.GetAllAsync();

        await _wordService.Received(1).GetByIdsAsync(Arg.Any<IEnumerable<int>>(), Arg.Any<string[]?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_CachedId_ReturnsEntity()
    {
        await _storage.SetAsync("selectedWordIds", new List<int> { 1 });
        SeedWordServiceReturns(Word(1));
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UncachedId_ReturnsNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task ContainsAsync_ReflectsCacheMembership()
    {
        await _storage.SetAsync("selectedWordIds", new List<int> { 1 });
        SeedWordServiceReturns(Word(1));
        var sut = CreateSut();

        Assert.True(await sut.ContainsAsync(1));
        Assert.False(await sut.ContainsAsync(2));
    }

    [Fact]
    public async Task AddAsync_NewId_FetchesCachesAndPersistsSelection()
    {
        SeedWordServiceReturns(Word(5));
        var sut = CreateSut();

        await sut.AddAsync(5);

        Assert.True(await sut.ContainsAsync(5));
        var persisted = await _storage.GetAsync<List<int>>("selectedWordIds");
        Assert.Contains(5, persisted);
    }

    [Fact]
    public async Task AddAsync_AlreadySelectedId_StillRefetches_DoesNotDuplicateSelection()
    {
        // Re-adding an already-selected id must refresh the cached entity (closes the staleness gap
        // from the library's README Known Issues), but must not add a duplicate entry to the
        // persisted selection list.
        await _storage.SetAsync("selectedWordIds", new List<int> { 1 });
        SeedWordServiceReturns(Word(1));
        var sut = CreateSut();
        await sut.GetAllAsync(); // first fetch

        await sut.AddAsync(1);

        await _wordService.Received(2).GetByIdsAsync(Arg.Any<IEnumerable<int>>(), Arg.Any<string[]?>(), Arg.Any<CancellationToken>());
        var persisted = await _storage.GetAsync<List<int>>("selectedWordIds");
        Assert.Single(persisted);
    }

    [Fact]
    public async Task RemoveAsync_DropsFromCacheAndPersistedSelection()
    {
        await _storage.SetAsync("selectedWordIds", new List<int> { 1, 2 });
        SeedWordServiceReturns(Word(1), Word(2));
        var sut = CreateSut();
        await sut.GetAllAsync();

        await sut.RemoveAsync(1);

        Assert.False(await sut.ContainsAsync(1));
        Assert.True(await sut.ContainsAsync(2));
        var persisted = await _storage.GetAsync<List<int>>("selectedWordIds");
        Assert.DoesNotContain(1, persisted);
    }

    [Fact]
    public async Task ClearCache_ResetsInMemoryState_ButRereadsUnchangedPersistedStorage()
    {
        await _storage.SetAsync("selectedWordIds", new List<int> { 1 });
        SeedWordServiceReturns(Word(1));
        var sut = CreateSut();
        await sut.GetAllAsync();

        sut.ClearCache();
        var result = await sut.GetAllAsync();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        await _wordService.Received(2).GetByIdsAsync(Arg.Any<IEnumerable<int>>(), Arg.Any<string[]?>(), Arg.Any<CancellationToken>());
    }
}
