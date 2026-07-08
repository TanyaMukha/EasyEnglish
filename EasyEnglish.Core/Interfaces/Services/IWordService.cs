using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IWordService : IBaseService<WordModel>
{
    Task<IEnumerable<WordModel>> GetAnyNextWordsAsync(int count);
    Task<IEnumerable<WordModel>> GetAnyHardWordsAsync(int count);
    Task<IEnumerable<WordModel>> GetByUnitAsync(int unitId, string[]? includes = null);
    Task<IEnumerable<WordModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);
    Task<WordModel> UpdateWordRateAsync(UpdateWordRateRequest word);
    Task<IEnumerable<WordModel>> UpdateWordRateRangeAsync(IEnumerable<UpdateWordRateRequest> words);
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentWordId);

    /// <summary>Number of words reviewed since the given point in time (by LastReviewDate).</summary>
    Task<int> CountReviewedSinceAsync(DateTime since);
}

public class UpdateWordRateRequest
{
    public int Id { get; init; }
    public float Rate { get; init; }
    public DateTime? LastReviewDate { get; init; }
    public int ReviewCount { get; init; }
}