using EasyEnglish.Core.Models;

namespace EasyEnglish.Core.Interfaces.Cache;

public interface IWordCacheService
{
    Task InitializeAsync();
    Task<List<WordModel>> GetAllAsync();
    Task<WordModel?> GetByIdAsync(int id);
    Task AddAsync(int wordId);
    Task RemoveAsync(int wordId);
    Task<bool> ContainsAsync(int wordId);
    void ClearCache();
}
