using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
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

    public async Task<IEnumerable<TestCardModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        var entities = await _testCardRepository.GetForLearningAsync(courseId, unitId, options);

        IEnumerable<TestCardEntity> result = entities;
        if (options.ShuffleWords)
            result = result.OrderBy(_ => Random.Shared.Next());

        return _mapper.Map<IEnumerable<TestCardModel>>(result);
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

    public async Task<IEnumerable<TestCardModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> cards)
    {
        try
        {
            var cardsList = cards.ToList();
            _logger.LogDebug("Оновлення рейтингу для {Count} тестових карток", cardsList.Count);

            var ids = cardsList.Select(c => c.Id).ToList();
            var entities = await _repository.FindManyAsync(ids);
            var entitiesDict = entities.ToDictionary(e => e.Id);

            foreach (var card in cardsList)
            {
                if (entitiesDict.TryGetValue(card.Id, out var entity))
                {
                    _mapper.Map(card, entity);
                }
            }

            await _repository.UpdateRangeAsync(entities);

            var updatedModels = _mapper.Map<IEnumerable<TestCardModel>>(entities);

            _logger.LogInformation("Оновлено рейтинг для {Count} тестових карток", cardsList.Count);
            return updatedModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні рейтингу тестових карток");
            throw;
        }
    }
}
