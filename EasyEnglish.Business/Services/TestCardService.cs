using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Business.Services;

/// <summary>Service for <see cref="TestCardModel"/>, beyond the generic CRUD in <see cref="BaseService{T, TModel}"/>.</summary>
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

    /// <inheritdoc/>
    /// <remarks>Shuffling happens here, after the repository query returns — not pushed down to SQL.</remarks>
    public async Task<IEnumerable<TestCardModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        var entities = await _testCardRepository.GetForLearningAsync(courseId, unitId, options);

        IEnumerable<TestCardEntity> result = entities;
        if (options.ShuffleWords)
            result = result.OrderBy(_ => Random.Shared.Next());

        return _mapper.Map<IEnumerable<TestCardModel>>(result);
    }

    /// <inheritdoc/>
    public Task<int> CountReviewedSinceAsync(DateTime since) => _testCardRepository.CountReviewedSinceAsync(since);

    /// <inheritdoc/>
    public async Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId)
    {
        return await _testCardRepository.GetNavigationIdsAsync(unitId, currentCardId);
    }

    /// <inheritdoc/>
    /// <remarks>Ids in <paramref name="cards"/> not found among existing rows are silently skipped, not reported.</remarks>
    public async Task<IEnumerable<TestCardModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> cards)
    {
        var cardsList = cards.ToList();
        _logger.LogDebug("Updating rating for {Count} test cards", cardsList.Count);

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

        _logger.LogInformation("Updated rating for {Count} test cards", cardsList.Count);
        return updatedModels;
    }
}
