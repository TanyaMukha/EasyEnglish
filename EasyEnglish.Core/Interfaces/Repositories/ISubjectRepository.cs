using EasyEnglish.Core.Entities;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface ISubjectRepository : IBaseRepository<SubjectEntity>
{
    /// <summary>Number of courses currently assigned to this subject.</summary>
    Task<int> CountCoursesAsync(int subjectId);
}
