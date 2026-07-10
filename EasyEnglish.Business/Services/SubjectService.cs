using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Services.Services;

public class SubjectService : BaseService<SubjectEntity, SubjectModel>, ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectService(
        ISubjectRepository repository,
        IMapper mapper,
        ILogger<SubjectService> logger)
        : base(repository, mapper, logger)
    {
        _subjectRepository = repository;
    }

    public Task<int> GetCourseCountAsync(int subjectId) => _subjectRepository.CountCoursesAsync(subjectId);
}
