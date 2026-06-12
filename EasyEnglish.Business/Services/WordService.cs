using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Options;
using EasyEnglish.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Services.Services;
public class WordService : BaseService<WordEntity, WordModel>, IWordService
{
    private readonly IWordRepository _wordRepository;

    public WordService(IWordRepository repository, IMapper mapper, ILogger<WordService> logger)
        : base(repository, mapper, logger)
    {
        _wordRepository = repository;
    }

    public async Task<IEnumerable<WordModel>> GetAnyNextWordsAsync(int count)
    {
        var entities = await _wordRepository.GetNextWordsAsync(count);
        return _mapper.Map<IEnumerable<WordModel>>(entities);
    }

    public async Task<IEnumerable<WordModel>> GetAnyHardWordsAsync(int count)
    {
        var entities = await _wordRepository.GetHardWordsAsync(count);
        return _mapper.Map<IEnumerable<WordModel>>(entities);
    }

    public async Task<IEnumerable<WordModel>> GetByUnitAsync(int unitId)
    {
        var entities = await _wordRepository.GetByUnitAsync(unitId);
        return _mapper.Map<IEnumerable<WordModel>>(entities);
    }

    public async Task<IEnumerable<WordModel>> GetForLearningAsync(int courseId, int? unitId, WordSelectionOptions options)
    {
        var entities = await _wordRepository.GetForLearningAsync(courseId, unitId, options);
        return _mapper.Map<IEnumerable<WordModel>>(entities);
    }

    public async Task<WordModel> UpdateWordRateAsync(UpdateWordRateRequest word)
    {
        WordModel? model = await this.GetByIdAsync(word.Id);
        if (model != null)
        {
            _mapper.Map(word, model);
        }

        return await this.UpdateAsync(model!.Id, model);
    }

    //public async Task<IEnumerable<WordModel>> UpdateWordRateRangeAsync(IEnumerable<UpdateWordRateRequest> words)
    //{
    //    List<WordModel> wordsToUpdate = new();

    //    foreach (var word in words)
    //    {
    //        WordModel? model = await this.GetByIdAsync(word.Id);
    //        if (model != null)
    //        {
    //            _mapper.Map(word, model);
    //            wordsToUpdate.Add(model);
    //        }
    //    }

    //    return await this.UpdateRangeAsync(wordsToUpdate.Select(w => (w.Id, w)));
    //}

    public async Task<IEnumerable<WordModel>> UpdateWordRateRangeAsync(IEnumerable<UpdateWordRateRequest> words)
    {
        try
        {
            _logger.LogDebug("Оновлення рейтингу для {Count} слів", words.Count());

            var wordsList = words.ToList();
            List<int> ids = wordsList.Select(w => w.Id).ToList() ?? new List<int>();

            // ✅ Отримуємо БЕЗ includes для update
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

            _logger.LogInformation("Оновлено рейтинг для {Count} слів", updatedModels.Count());
            return updatedModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні рейтингу слів");
            throw;
        }
    }

    public async Task<(int? PreviousId, int? NextId)> GetNavigationIdsAsync(int unitId, int currentWordId)
    {
        try
        {
            _logger.LogDebug("Отримання ID сусідніх слів для слова {WordId} у модулі {UnitId}", currentWordId, unitId);

            var navigationIds = await this._wordRepository.GetNavigationIdsAsync(unitId, currentWordId);

            _logger.LogDebug("Попереднє слово: {PreviousId}, Наступне слово: {NextId}",
                navigationIds.PreviousId, navigationIds.NextId);

            return navigationIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні ID сусідніх слів для слова {WordId}", currentWordId);
            throw;
        }
    }
}