using EasyEnglish.Core.Models;

namespace EasyEnglish.Core.Interfaces.Cache;

public interface ICurrentUnitCacheService
{
    Task InitializeAsync();
    Task<UnitModel?> GetAsync();
    Task SetAsync(int unitId);
    Task ClearAsync();
    Task<bool> HasValueAsync();
    void ClearCache();
}
