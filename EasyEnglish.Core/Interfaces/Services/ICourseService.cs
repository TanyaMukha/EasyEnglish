using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;

namespace EasyEnglish.Core.Interfaces.Services;

public interface ICourseService : IBaseService<CourseModel>
{
    Task<IEnumerable<UnitModel>> GetUnitsAsync(int courseId);

    Task<IEnumerable<WordModel>> GetWordsAsync(int courseId, int? unitId = null, WordSelectionOptions? options = null);
}