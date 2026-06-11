using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IUnitService : IBaseWithGuidService<UnitModel>
{
    Task<IReadOnlyList<UnitCardModel>> GetUnitCardsAsync(int courseId);

    Task<IEnumerable<WordModel>> GetWordsAsync(int unitId);
}