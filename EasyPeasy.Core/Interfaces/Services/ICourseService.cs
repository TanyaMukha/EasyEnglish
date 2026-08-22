using EasyPeasy.Core.Models;
using EasyPeasy.Core.Options;
using MukhaLab.Database;

namespace EasyPeasy.Core.Interfaces.Services;

/// <summary>Service for <see cref="CourseModel"/>, beyond the generic CRUD in <see cref="IBaseWithGuidService{TModel}"/>.</summary>
public interface ICourseService : IBaseWithGuidService<CourseModel>
{
    /// <summary>All units belonging to a course.</summary>
    Task<IEnumerable<UnitModel>> GetUnitsAsync(int courseId);

    /// <summary>
    /// Words from a course, optionally scoped to one unit and filtered/selected according to
    /// <paramref name="options"/> (falls back to <see cref="LearningSelectionOptions"/> defaults if omitted).
    /// </summary>
    Task<IEnumerable<WordModel>> GetWordsAsync(int courseId, int? unitId = null, LearningSelectionOptions? options = null);
}