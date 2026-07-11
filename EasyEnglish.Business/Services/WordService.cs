using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Services.Services;

/// <summary>Service for <see cref="WordModel"/>, beyond the generic CRUD in <see cref="BaseService{T, TModel}"/>.</summary>
public class WordService : BaseService<WordEntity, WordModel>, IWordService
{
    private readonly IWordRepository _wordRepository;

    public WordService(IWordRepository repository, IMapper mapper, ILogger<WordService> logger)
        : base(repository, mapper, logger)
    {
        _wordRepository = repository;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WordModel>> GetAnyNextWordsAsync(int count)
    {
        var entities = await _wordRepository.GetNextWordsAsync(count);
        return _mapper.Map<IEnumerable<WordModel>>(entities);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WordModel>> GetAnyHardWordsAsync(int count)
    {
        var entities = await _wordRepository.GetHardWordsAsync(count);
        return _mapper.Map<IEnumerable<WordModel>>(entities);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WordModel>> GetByUnitAsync(int unitId, string[]? includes = null)
    {
        var entities = await _wordRepository.GetByUnitAsync(unitId, includes);
        return _mapper.Map<IEnumerable<WordModel>>(entities);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WordModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        var entities = await _wordRepository.GetForLearningAsync(courseId, unitId, options);
        return _mapper.Map<IEnumerable<WordModel>>(entities);
    }

    /// <inheritdoc/>
    public Task<int> CountReviewedSinceAsync(DateTime since) => _wordRepository.CountReviewedSinceAsync(since);

    /// <inheritdoc/>
    /// <remarks>
    /// If <paramref name="word"/>.Id doesn't match an existing word, <c>model</c> stays <c>null</c> and
    /// <c>model!.Id</c> below throws <see cref="NullReferenceException"/> instead of a clear
    /// not-found signal. See EasyEnglish.Business/README.md Known Issues.
    /// </remarks>
    public async Task<WordModel> UpdateWordRateAsync(UpdateWordRateRequest word)
    {
        WordModel? model = await this.GetByIdAsync(word.Id);
        if (model != null)
        {
            _mapper.Map(word, model);
        }

        return await this.UpdateAsync(model!.Id, model);
    }

    /// <inheritdoc/>
    /// <remarks>Ids in <paramref name="words"/> not found among existing rows are silently skipped, not reported.</remarks>
    public async Task<IEnumerable<WordModel>> UpdateWordRateRangeAsync(IEnumerable<UpdateWordRateRequest> words)
    {
        try
        {
            _logger.LogDebug("Updating rating for {Count} words", words.Count());

            var wordsList = words.ToList();
            List<int> ids = wordsList.Select(w => w.Id).ToList() ?? new List<int>();

            // Fetch WITHOUT includes for the update
            var entities = await _repository.FindManyAsync(ids);

            var entitiesDict = entities.ToDictionary(e => e.Id);

            foreach (var word in wordsList)
            {
                if (entitiesDict.TryGetValue(word.Id, out var entity))
                {
                    _mapper.Map(word, entity);
                }
            }

            await _repository.UpdateRangeAsync(entities);

            var updatedModels = _mapper.Map<IEnumerable<WordModel>>(entities);

            _logger.LogInformation("Updated rating for {Count} words", updatedModels.Count());
            return updatedModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating word ratings");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentWordId)
    {
        try
        {
            _logger.LogDebug("Fetching neighboring word ids for word {WordId} in unit {UnitId}", currentWordId, unitId);

            var navigationIds = await this._wordRepository.GetNavigationIdsAsync(unitId, currentWordId);

            _logger.LogDebug("Previous word: {PreviousId}, next word: {NextId}, position: {Position}/{Total}",
                navigationIds.PreviousId, navigationIds.NextId, navigationIds.Position, navigationIds.Total);

            return navigationIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching neighboring word ids for word {WordId}", currentWordId);
            throw;
        }
    }
}