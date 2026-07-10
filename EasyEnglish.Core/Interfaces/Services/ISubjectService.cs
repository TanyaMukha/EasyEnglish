using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface ISubjectService : IBaseService<SubjectModel>
{
    /// <summary>Number of courses currently assigned to this subject.</summary>
    Task<int> GetCourseCountAsync(int subjectId);
}
