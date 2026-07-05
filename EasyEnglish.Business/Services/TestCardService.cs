using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Services.Services;

public class TestCardService : BaseService<TestCardEntity, TestCardModel>, ITestCardService
{
    public TestCardService(
        ITestCardRepository repository,
        IMapper mapper,
        ILogger<TestCardService> logger)
        : base(repository, mapper, logger)
    {
    }
}
