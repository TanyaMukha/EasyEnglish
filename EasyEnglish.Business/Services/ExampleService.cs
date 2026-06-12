using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Services.Services;

public class ExampleService : BaseService<ExampleEntity, ExampleModel>, IExampleService
{
    public ExampleService(
        IExampleRepository repository,
        IMapper mapper,
        ILogger<ExampleService> logger)
        : base(repository, mapper, logger)
    {
    }
}
