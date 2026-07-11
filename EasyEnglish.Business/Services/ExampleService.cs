using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Business.Services;

/// <summary>Service for <see cref="ExampleModel"/>, beyond the generic CRUD in <see cref="BaseService{T, TModel}"/>.</summary>
public class ExampleService : BaseService<ExampleEntity, ExampleModel>, IExampleService
{
    private readonly IExampleRepository _exampleRepository;

    public ExampleService(
        IExampleRepository repository,
        IMapper mapper,
        ILogger<ExampleService> logger)
        : base(repository, mapper, logger)
    {
        _exampleRepository = repository;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ExampleModel>> GetByUnitAsync(int unitId)
    {
        var entities = await _exampleRepository.GetByUnitAsync(unitId);
        return _mapper.Map<IEnumerable<ExampleModel>>(entities);
    }
}
