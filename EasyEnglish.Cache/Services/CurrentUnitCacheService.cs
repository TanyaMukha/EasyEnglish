using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Storage;
using EasyEnglish.Core.Interfaces.Cache;

namespace EasyEnglish.Cache.Services;

public class CurrentUnitCacheService : BaseSingleCacheService<UnitModel, int>, ICurrentUnitCacheService
{
    private readonly IUnitService _unitService;

    public CurrentUnitCacheService(
        IStorageService storage,
        IUnitService unitService)
        : base(storage)
    {
        _unitService = unitService;
    }

    protected override string StorageKey => "currentUnitId";

    protected override Task<UnitModel?> FetchEntityAsync(int id)
    {
        return _unitService.GetByIdAsync(id); // якщо такий метод є
    }

    protected override int GetEntityId(UnitModel entity)
    {
        return entity.Id;
    }
}
