using EasyEnglish.Core.Models;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IWordService : IBaseService<WordModel>
{
    Task<IEnumerable<WordModel>> GetAnyNextWordsAsync(int count);
    Task<IEnumerable<WordModel>> GetAnyHardWordsAsync(int count);
    Task<WordModel> UpdateWordRateAsync(UpdateWordRateRequest word);
    Task<IEnumerable<WordModel>> UpdateWordRateRangeAsync(IEnumerable<UpdateWordRateRequest> words);
    Task<(int? PreviousId, int? NextId)> GetNavigationIdsAsync(int unitId, int currentWordId);
}

public class UpdateWordRateRequest
{
    public int Id { get; init; }
    public float Rate { get; init; }
    public DateTime? LastReviewDate { get; init; }
    public int ReviewCount { get; init; }
}