using EasyEnglish.Core.Entities;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

/// <summary>Repository for <see cref="SubjectEntity"/>, beyond the generic CRUD in <see cref="IBaseRepository{T}"/>.</summary>
public interface ISubjectRepository : IBaseRepository<SubjectEntity>
{
    /// <summary>Number of courses currently assigned to this subject.</summary>
    Task<int> CountCoursesAsync(int subjectId);
}
