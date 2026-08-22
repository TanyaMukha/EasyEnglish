using AutoMapper;
using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Interfaces.Repositories;
using EasyPeasy.Core.Interfaces.Services;
using EasyPeasy.Core.Models;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyPeasy.Business.Services;

/// <summary>Service for <see cref="SubjectModel"/>, beyond the generic CRUD in <see cref="BaseService{T, TModel}"/>.</summary>
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

    /// <inheritdoc/>
    public Task<int> GetCourseCountAsync(int subjectId) => _subjectRepository.CountCoursesAsync(subjectId);
}
