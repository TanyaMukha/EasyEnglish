using EasyPeasy.Core.Interfaces.Services;
using EasyPeasy.Core.Models;
using EasyPeasy.Core.Interfaces.Storage;
using EasyPeasy.Core.Interfaces.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace EasyPeasy.Cache.Services;

/// <summary>Caches the single "currently open" unit — see <see cref="BaseSingleCacheService{TEntity, TId}"/> for the caching semantics.</summary>
public class CurrentUnitCacheService : BaseSingleCacheService<UnitModel, int>, ICurrentUnitCacheService
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <param name="storage">Persistent storage for the selected unit id.</param>
    /// <param name="scopeFactory">
    /// Used to resolve <see cref="IUnitService"/> in a short-lived scope per fetch, instead of
    /// injecting it directly — <see cref="IUnitService"/> is registered scoped, while this service is
    /// a singleton, and a singleton must not hold a scoped dependency directly (captive dependency).
    /// </param>
    public CurrentUnitCacheService(
        IStorageService storage,
        IServiceScopeFactory scopeFactory)
        : base(storage)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string StorageKey => "currentUnitId";

    protected override async Task<UnitModel?> FetchEntityAsync(int id)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitService = scope.ServiceProvider.GetRequiredService<IUnitService>();
        return await unitService.GetByIdAsync(id);
    }

    protected override int GetEntityId(UnitModel entity)
    {
        return entity.Id;
    }
}
