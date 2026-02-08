using EasyEnglish.Core.Entities;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface IWordRepository : IBaseRepository<WordEntity>
{
    Task<(int? PreviousId, int? NextId)> GetNavigationIdsAsync(int unitId, int currentWordId);
}
