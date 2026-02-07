using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.Cache.Services;

public abstract class BaseCacheService<TEntity, TId>
    where TEntity : class
    where TId : notnull
{
    private readonly IStorageService _storage;
    protected Dictionary<TId, TEntity> Cache = new();
    protected List<TId> SelectedIds = new();
    private bool _isInitialized = false;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    protected abstract string StorageKey { get; }
    protected abstract Task<List<TEntity>> FetchEntitiesAsync(List<TId> ids);
    protected abstract TId GetEntityId(TEntity entity);

    protected BaseCacheService(IStorageService storage)
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

    public async Task<List<TEntity>> GetAllAsync()
    {
        await InitializeAsync();
        return SelectedIds
            .Where(id => Cache.ContainsKey(id))
            .Select(id => Cache[id])
            .ToList();
    }

    public async Task<TEntity?> GetByIdAsync(TId id)
    {
        await InitializeAsync();
        return Cache.GetValueOrDefault(id);
    }

    public async Task AddAsync(TId id)
    {
        await InitializeAsync();

        if (SelectedIds.Contains(id))
            return;

        SelectedIds.Add(id);
        await _storage.SetAsync(StorageKey, SelectedIds);

        if (!Cache.ContainsKey(id))
        {
            var entities = await FetchEntitiesAsync(new List<TId> { id });
            var entity = entities.FirstOrDefault();
            if (entity != null)
            {
                Cache[GetEntityId(entity)] = entity;
            }
        }
    }

    public async Task RemoveAsync(TId id)
    {
        await InitializeAsync();

        SelectedIds.Remove(id);
        await _storage.SetAsync(StorageKey, SelectedIds);
        Cache.Remove(id);
    }

    public void ClearCache()
    {
        Cache.Clear();
        SelectedIds.Clear();
        _isInitialized = false;
    }

    public async Task<bool> ContainsAsync(TId id)
    {
        await InitializeAsync();
        return Cache.ContainsKey(id);
    }
}
