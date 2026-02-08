using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using EasyEnglish.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MukhaLab.SelectQueryParameters.Models;

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
        QueryParameters parameters = new QueryParameters
        {
            PageNumber = 1,
            RowCount = count,
            Sort = new List<SortDescriptor>
            {
                new SortDescriptor { Field = "LastReviewDate", Direction = SortDirection.Asc }
            }           
        };

        return await this.GetAllAsync(parameters);
    }

    public async Task<IEnumerable<WordModel>> GetAnyHardWordsAsync(int count)
    {
        QueryParameters parameters = new QueryParameters
        {
            PageNumber = 1,
            RowCount = count,
            Sort = new List<SortDescriptor>
            {
                new SortDescriptor { Field = "Rate", Direction = SortDirection.Desc }
            }
        };

        return await this.GetAllAsync(parameters);
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

    public async Task<IEnumerable<WordModel>> UpdateWordRateRangeAsync(IEnumerable<UpdateWordRateRequest> words)
    {
        List<WordModel> wordsToUpdate = new();

        foreach (var word in words)
        {
            WordModel? model = await this.GetByIdAsync(word.Id);
            if (model != null)
            {
                _mapper.Map(word, model);
                wordsToUpdate.Add(model);
            }
        }

        return await this.UpdateRangeAsync(wordsToUpdate.Select(w => (w.Id, w)));
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