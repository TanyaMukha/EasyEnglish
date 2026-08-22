using EasyPeasy.Core.Models;
using MukhaLab.Database;

namespace EasyPeasy.Core.Interfaces.Services;

/// <summary>Service for <see cref="SubjectModel"/>, beyond the generic CRUD in <see cref="IBaseService{TModel}"/>.</summary>
public interface ISubjectService : IBaseService<SubjectModel>
{
    /// <summary>Number of courses currently assigned to this subject.</summary>
    Task<int> GetCourseCountAsync(int subjectId);
}
