using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface ICourseService : IBaseWithGuidService<CourseModel>
{
    Task<IEnumerable<UnitModel>> GetUnitsAsync(int courseId);

    Task<IEnumerable<WordModel>> GetWordsAsync(int courseId, int? unitId = null, LearningSelectionOptions? options = null);
}