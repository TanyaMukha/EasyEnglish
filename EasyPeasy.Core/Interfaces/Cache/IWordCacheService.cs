using EasyPeasy.Core.Models;

namespace EasyPeasy.Core.Interfaces.Cache;

/// <summary>Caches a working set of <see cref="WordModel"/> records (e.g. the current learning session's words).</summary>
public interface IWordCacheService
{
    /// <summary>Loads the cached word set from persistent storage into memory, if any was saved.</summary>
    Task InitializeAsync();

    /// <summary>Returns all currently cached words.</summary>
    Task<List<WordModel>> GetAllAsync();

    /// <summary>Returns a single cached word by id, or <c>null</c> if it isn't cached.</summary>
    Task<WordModel?> GetByIdAsync(int id);

    /// <summary>Fetches a word by id and adds it to the cache.</summary>
    Task AddAsync(int wordId);

    /// <summary>Removes a word from the cache by id.</summary>
    Task RemoveAsync(int wordId);

    /// <summary>Whether a word with the given id is currently cached.</summary>
    Task<bool> ContainsAsync(int wordId);

    /// <summary>Clears only the in-memory cache, without touching persistent storage.</summary>
    void ClearCache();
}
