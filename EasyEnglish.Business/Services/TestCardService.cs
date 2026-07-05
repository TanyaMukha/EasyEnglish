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
    private readonly ITestCardRepository _testCardRepository;

    public TestCardService(
        ITestCardRepository repository,
        IMapper mapper,
        ILogger<TestCardService> logger)
        : base(repository, mapper, logger)
    {
        _testCardRepository = repository;
    }

    public async Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId)
    {
        try
        {
            _logger.LogDebug("Отримання ID сусідніх тестових карток для картки {CardId} у модулі {UnitId}", currentCardId, unitId);
            return await _testCardRepository.GetNavigationIdsAsync(unitId, currentCardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні ID сусідніх тестових карток для картки {CardId}", currentCardId);
            throw;
        }
    }
}
