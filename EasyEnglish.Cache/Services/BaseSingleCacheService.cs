using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.Cache.Services;

public abstract class BaseSingleCacheService<TEntity, TId>
    where TEntity : class
    where TId : notnull
{
    private readonly IStorageService _storage;
    protected TEntity? CachedEntity = null;
    protected TId? SelectedId = default;
    private bool _isInitialized = false;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    protected abstract string StorageKey { get; }
    protected abstract Task<TEntity?> FetchEntityAsync(TId id);
    protected abstract TId GetEntityId(TEntity entity);

    protected BaseSingleCacheService(IStorageService storage)
    {
        _storage = storage;
    }

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

    public async Task<TEntity?> GetAsync()
    {
        await InitializeAsync();
        return CachedEntity;
    }

    public async Task<TId?> GetIdAsync()
    {
        await InitializeAsync();
        return SelectedId;
    }

    public async Task SetAsync(TId id)
    {
        await InitializeAsync();

        if (SelectedId != null && SelectedId.Equals(id))
            return;

        SelectedId = id;
        await _storage.SetAsync(StorageKey, id);

        // Завантажити сутність з БД в кеш
        CachedEntity = await FetchEntityAsync(id);
    }

    public async Task ClearAsync()
    {
        await InitializeAsync();

        SelectedId = default;
        CachedEntity = null;
        await _storage.SetAsync<TId?>(StorageKey, default);
    }

    public void ClearCache()
    {
        CachedEntity = null;
        SelectedId = default;
        _isInitialized = false;
    }

    public async Task<bool> HasValueAsync()
    {
        await InitializeAsync();
        return CachedEntity != null;
    }
}
