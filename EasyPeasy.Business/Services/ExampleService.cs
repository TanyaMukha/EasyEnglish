using AutoMapper;
using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Interfaces.Repositories;
using EasyPeasy.Core.Interfaces.Services;
using EasyPeasy.Core.Models;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyPeasy.Business.Services;

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
