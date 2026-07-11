using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Services.Services;

/// <summary>Service for <see cref="StudyCardModel"/>, beyond the generic CRUD in <see cref="BaseService{T, TModel}"/>.</summary>
public class StudyCardService : BaseService<StudyCardEntity, StudyCardModel>, IStudyCardService
{
    private readonly IStudyCardRepository _studyCardRepository;

    public StudyCardService(
        IStudyCardRepository repository,
        IMapper mapper,
        ILogger<StudyCardService> logger)
        : base(repository, mapper, logger)
    {
        _studyCardRepository = repository;
    }

    /// <inheritdoc/>
    /// <remarks>Shuffling happens here, after the repository query returns — not pushed down to SQL.</remarks>
    public async Task<IEnumerable<StudyCardModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        var entities = await _studyCardRepository.GetForLearningAsync(courseId, unitId, options);

        IEnumerable<StudyCardEntity> result = entities;
        if (options.ShuffleWords)
            result = result.OrderBy(_ => Random.Shared.Next());

        return _mapper.Map<IEnumerable<StudyCardModel>>(result);
    }

    /// <inheritdoc/>
    public Task<int> CountReviewedSinceAsync(DateTime since) => _studyCardRepository.CountReviewedSinceAsync(since);

    /// <inheritdoc/>
    public async Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId)
    {
        try
        {
            _logger.LogDebug("Fetching neighboring study card ids for card {CardId} in unit {UnitId}", currentCardId, unitId);
            return await _studyCardRepository.GetNavigationIdsAsync(unitId, currentCardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching neighboring study card ids for card {CardId}", currentCardId);
            throw;
        }
    }

    /// <inheritdoc/>
    /// <remarks>Ids in <paramref name="cards"/> not found among existing rows are silently skipped, not reported.</remarks>
    public async Task<IEnumerable<StudyCardModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> cards)
    {
        try
        {
            var cardsList = cards.ToList();
            _logger.LogDebug("Updating rating for {Count} study cards", cardsList.Count);

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

            var updatedModels = _mapper.Map<IEnumerable<StudyCardModel>>(entities);

            _logger.LogInformation("Updated rating for {Count} study cards", cardsList.Count);
            return updatedModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating study card ratings");
            throw;
        }
    }
}
