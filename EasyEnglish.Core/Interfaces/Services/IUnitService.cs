using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IUnitService : IBaseWithGuidService<UnitModel>
{
    Task<IReadOnlyList<UnitCardModel>> GetUnitCardsAsync(int courseId);

    Task<IEnumerable<UnitModel>> GetByCourseAsync(int courseId);

    Task<IEnumerable<WordModel>> GetWordsAsync(int unitId, string[]? includes = null);

    /// <summary>All examples belonging to a unit's words, as a flat list.</summary>
    Task<IEnumerable<ExampleModel>> GetExamplesAsync(int unitId);
}