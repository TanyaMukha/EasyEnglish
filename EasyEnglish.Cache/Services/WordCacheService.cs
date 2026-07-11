using EasyEnglish.Core.Interfaces.Cache;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EasyEnglish.Cache.Services;

/// <summary>Caches a working set of words — see <see cref="BaseCacheService{TEntity, TId}"/> for the caching semantics.</summary>
public class WordCacheService : BaseCacheService<WordModel, int>, IWordCacheService
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <param name="storage">Persistent storage for the selected word ids.</param>
    /// <param name="scopeFactory">
    /// Used to resolve <see cref="IWordService"/> in a short-lived scope per fetch, instead of
    /// injecting it directly — <see cref="IWordService"/> is registered scoped, while this service is
    /// a singleton, and a singleton must not hold a scoped dependency directly (captive dependency).
    /// </param>
    public WordCacheService(
        IStorageService storage,
        IServiceScopeFactory scopeFactory)
        : base(storage)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string StorageKey => "selectedWordIds";

    protected override async Task<List<WordModel>> FetchEntitiesAsync(List<int> ids)
    {
        using var scope = _scopeFactory.CreateScope();
        var wordService = scope.ServiceProvider.GetRequiredService<IWordService>();
        return await wordService.GetByIdsAsync(ids);
    }

    protected override int GetEntityId(WordModel entity)
    {
        return entity.Id;
    }
}
