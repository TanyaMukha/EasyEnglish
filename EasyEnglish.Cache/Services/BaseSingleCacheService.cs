using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.Cache.Services;

/// <summary>
/// In-memory cache for a single "current" entity (e.g. the unit the user has open), whose selected
/// id persists across app restarts via <see cref="IStorageService"/>. Lazily loads on first use via
/// <see cref="InitializeAsync"/>, guarded by a <see cref="SemaphoreSlim"/> so concurrent first calls
/// don't double-fetch.
/// </summary>
public abstract class BaseSingleCacheService<TEntity, TId>
    where TEntity : class
    where TId : notnull
{
    private readonly IStorageService _storage;
    protected TEntity? CachedEntity = null;
    protected TId? SelectedId = default;
    private bool _isInitialized = false;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    /// <summary>The <see cref="IStorageService"/> key under which the selected id is persisted.</summary>
    protected abstract string StorageKey { get; }

    /// <summary>Loads the entity by id from the real data source (not the cache) — called on init and whenever the selected id changes.</summary>
    protected abstract Task<TEntity?> FetchEntityAsync(TId id);

    /// <summary>Extracts an entity's id. Not currently called anywhere in this class — reserved for subclasses/future use.</summary>
    protected abstract TId GetEntityId(TEntity entity);

    protected BaseSingleCacheService(IStorageService storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Loads the persisted selected id and its entity on first call; a no-op on every call after
    /// that (until <see cref="ClearCache"/> resets it). Every other public method calls this first,
    /// so callers don't need to call it explicitly.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized) return;

            SelectedId = await _storage.GetAsync<TId>(StorageKey);

            if (SelectedId != null && !SelectedId.Equals(default(TId)))
            {
                CachedEntity = await FetchEntityAsync(SelectedId);
            }

            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>Returns the cached entity, or <c>null</c> if none is selected.</summary>
    public async Task<TEntity?> GetAsync()
    {
        await InitializeAsync();
        return CachedEntity;
    }

    /// <summary>Returns the currently selected id, or <c>default</c> if none is selected.</summary>
    public async Task<TId?> GetIdAsync()
    {
        await InitializeAsync();
        return SelectedId;
    }

    /// <summary>
    /// Selects <paramref name="id"/> (persisting it, if it wasn't already selected) and
    /// (re)fetches its entity into <see cref="CachedEntity"/> — even if <paramref name="id"/> was
    /// already selected, so a stale cached copy never lingers just because it was selected once before.
    /// </summary>
    public async Task SetAsync(TId id)
    {
        await InitializeAsync();

        if (SelectedId == null || !SelectedId.Equals(id))
        {
            SelectedId = id;
            await _storage.SetAsync(StorageKey, id);
        }

        CachedEntity = await FetchEntityAsync(id);
    }

    /// <summary>Deselects the current entity, in memory and in persistent storage.</summary>
    public async Task ClearAsync()
    {
        await InitializeAsync();

        SelectedId = default;
        CachedEntity = null;
        await _storage.SetAsync<TId?>(StorageKey, default);
    }

    /// <summary>
    /// Clears only the in-memory cache and resets initialization state — does not touch persistent
    /// storage. The next call to any other method re-triggers <see cref="InitializeAsync"/>, which
    /// re-reads the (untouched) persisted selection and refetches its entity.
    /// </summary>
    public void ClearCache()
    {
        CachedEntity = null;
        SelectedId = default;
        _isInitialized = false;
    }

    /// <summary>Whether an entity is currently selected and cached.</summary>
    public async Task<bool> HasValueAsync()
    {
        await InitializeAsync();
        return CachedEntity != null;
    }
}
