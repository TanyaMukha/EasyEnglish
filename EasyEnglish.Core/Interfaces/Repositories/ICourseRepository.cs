using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

/// <summary>
/// Repository for <see cref="CourseEntity"/>. Adds no members of its own — exists to give the
/// generic <see cref="IBaseWithGuidRepository{T}"/> a course-specific DI registration point.
/// </summary>
public interface ICourseRepository : IBaseWithGuidRepository<CourseEntity>
{
}
