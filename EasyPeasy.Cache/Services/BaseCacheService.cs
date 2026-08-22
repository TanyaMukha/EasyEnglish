using EasyPeasy.Core.Interfaces.Storage;

namespace EasyPeasy.Cache.Services;

/// <summary>
/// In-memory cache for a working set of entities, keyed by id, whose membership (which ids are
/// "selected") persists across app restarts via <see cref="IStorageService"/>. Distinct from a
/// generic LRU/expiry cache — entries never leave the set on their own; only <see cref="RemoveAsync"/>
/// or <see cref="ClearCache"/> drops them. Lazily loads on first use via <see cref="InitializeAsync"/>,
/// guarded by a <see cref="SemaphoreSlim"/> so concurrent first calls don't double-fetch.
/// </summary>
public abstract class BaseCacheService<TEntity, TId>
    where TEntity : class
    where TId : notnull
{
    private readonly IStorageService _storage;
    protected Dictionary<TId, TEntity> Cache = new();
    protected List<TId> SelectedIds = new();
    private bool _isInitialized = false;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    /// <summary>The <see cref="IStorageService"/> key under which the selected id list is persisted.</summary>
    protected abstract string StorageKey { get; }

    /// <summary>Loads entities by id from the real data source (not the cache) — called on init and whenever a new id is added.</summary>
    protected abstract Task<List<TEntity>> FetchEntitiesAsync(List<TId> ids);

    /// <summary>Extracts an entity's id, for indexing it into <see cref="Cache"/>.</summary>
    protected abstract TId GetEntityId(TEntity entity);

    protected BaseCacheService(IStorageService storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Loads the persisted id list and the corresponding entities on first call; a no-op on every
    /// call after that (until <see cref="ClearCache"/> resets it). Every other public method calls
    /// this first, so callers don't need to call it explicitly.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized) return;

            SelectedIds = await _storage.GetAsync<List<TId>>(StorageKey)
                ?? new List<TId>();

            if (SelectedIds.Any())
            {
                var entities = await FetchEntitiesAsync(SelectedIds);
                Cache = entities.ToDictionary(GetEntityId);
            }

            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>All currently cached entities, in <see cref="SelectedIds"/> order. Ids present in the persisted selection but missing from <see cref="Cache"/> (e.g. deleted upstream) are silently skipped.</summary>
    public async Task<List<TEntity>> GetAllAsync()
    {
        await InitializeAsync();
        return SelectedIds
            .Where(id => Cache.ContainsKey(id))
            .Select(id => Cache[id])
            .ToList();
    }

    /// <summary>Returns a single cached entity by id, or <c>null</c> if it isn't in the cache.</summary>
    public async Task<TEntity?> GetByIdAsync(TId id)
    {
        await InitializeAsync();
        return Cache.GetValueOrDefault(id);
    }

    /// <summary>
    /// Adds <paramref name="id"/> to the persisted selection (if not already present) and
    /// (re)fetches/caches its entity — even if <paramref name="id"/> was already selected, so a
    /// stale cached copy never lingers just because it was added once before.
    /// </summary>
    public async Task AddAsync(TId id)
    {
        await InitializeAsync();

        if (!SelectedIds.Contains(id))
        {
            SelectedIds.Add(id);
            await _storage.SetAsync(StorageKey, SelectedIds);
        }

        var entities = await FetchEntitiesAsync(new List<TId> { id });
        var entity = entities.FirstOrDefault();
        if (entity != null)
        {
            Cache[GetEntityId(entity)] = entity;
        }
    }

    /// <summary>Removes <paramref name="id"/> from the persisted selection and drops its cached entity.</summary>
    public async Task RemoveAsync(TId id)
    {
        await InitializeAsync();

        SelectedIds.Remove(id);
        await _storage.SetAsync(StorageKey, SelectedIds);
        Cache.Remove(id);
    }

    /// <summary>
    /// Clears only the in-memory cache and resets initialization state — does not touch persistent
    /// storage. The next call to any other method re-triggers <see cref="InitializeAsync"/>, which
    /// re-reads the (untouched) persisted selection.
    /// </summary>
    public void ClearCache()
    {
        Cache.Clear();
        SelectedIds.Clear();
        _isInitialized = false;
    }

    /// <summary>Whether <paramref name="id"/> is currently cached.</summary>
    public async Task<bool> ContainsAsync(TId id)
    {
        await InitializeAsync();
        return Cache.ContainsKey(id);
    }
}
