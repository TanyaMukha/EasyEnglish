using EasyEnglish.Core.Models;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IUnitService : IBaseService<UnitModel>
{
    Task<IEnumerable<WordModel>> GetWordsAsync(int unitId);
}