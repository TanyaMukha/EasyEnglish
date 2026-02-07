using EasyEnglish.Core.Interfaces.Cache;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.Cache.Services;

public class WordCacheService : BaseCacheService<WordModel, int>, IWordCacheService
{
    private readonly IWordService _wordService;

    public WordCacheService(
        IStorageService storage,
        IWordService wordService)
        : base(storage)
    {
        _wordService = wordService;
    }

    protected override string StorageKey => "selectedWordIds";

    protected override Task<List<WordModel>> FetchEntitiesAsync(List<int> ids)
    {
        return _wordService.GetByIdsAsync(ids);
    }

    protected override int GetEntityId(WordModel entity)
    {
        return entity.Id;
    }
}
