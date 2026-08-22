using EasyPeasy.Core.Models;

namespace EasyPeasy.Core.Interfaces.Cache;

/// <summary>Caches the single "currently open" <see cref="UnitModel"/> across the app session.</summary>
public interface ICurrentUnitCacheService
{
    /// <summary>Loads the cached unit from persistent storage into memory, if any was saved.</summary>
    Task InitializeAsync();

    /// <summary>Returns the cached unit, or <c>null</c> if none is set.</summary>
    Task<UnitModel?> GetAsync();

    /// <summary>Sets the current unit by id, fetching and caching it.</summary>
    Task SetAsync(int unitId);

    /// <summary>Clears the cached unit, in memory and in persistent storage.</summary>
    Task ClearAsync();

    /// <summary>Whether a unit is currently cached.</summary>
    Task<bool> HasValueAsync();

    /// <summary>Clears only the in-memory cache, without touching persistent storage.</summary>
    void ClearCache();
}
