using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface IWordRepository : IBaseRepository<WordEntity>
{
    Task<(int? PreviousId, int? NextId)> GetNavigationIdsAsync(int unitId, int currentWordId);

    /// <summary>Слова, що найдовше не повторювались.</summary>
    Task<List<WordEntity>> GetNextWordsAsync(int count);

    /// <summary>Найскладніші слова (за рейтингом).</summary>
    Task<List<WordEntity>> GetHardWordsAsync(int count);

    /// <summary>Усі слова юніта.</summary>
    Task<List<WordEntity>> GetByUnitAsync(int unitId);

    /// <summary>Добірка слів курсу/юніта для вивчення згідно з опціями.</summary>
    Task<List<WordEntity>> GetForLearningAsync(int courseId, int? unitId, WordSelectionOptions options);
}
