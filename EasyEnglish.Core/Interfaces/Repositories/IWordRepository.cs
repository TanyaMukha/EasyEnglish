using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface IWordRepository : IBaseRepository<WordEntity>
{
    Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentWordId);

    /// <summary>Words that haven't been reviewed for the longest time.</summary>
    Task<List<WordEntity>> GetNextWordsAsync(int count);

    /// <summary>The most difficult words (by rating).</summary>
    Task<List<WordEntity>> GetHardWordsAsync(int count);

    /// <summary>All words in a unit.</summary>
    Task<List<WordEntity>> GetByUnitAsync(int unitId, string[]? includes = null);

    /// <summary>Selects words from a course/unit for learning, according to the given options.</summary>
    Task<List<WordEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options);
}
