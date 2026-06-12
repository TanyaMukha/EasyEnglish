using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Services.Services;

public class IrregularFormService : BaseService<IrregularFormEntity ,IrregularFormModel>, IIrregularFormService
{
    public IrregularFormService(
        IIrregularFormRepository repository,
        IMapper mapper,
        ILogger<IrregularFormService> logger)
        : base(repository, mapper, logger)
    {
    }

    public async Task<IEnumerable<IrregularFormModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> forms)
    {
        try
        {
            var formsList = forms.ToList();
            _logger.LogDebug("Оновлення рейтингу для {Count} неправильних форм", formsList.Count);

            List<int> ids = formsList.Select(f => f.Id).ToList();

            // Отримуємо БЕЗ includes для update
            var entities = await _repository.FindManyAsync(ids);

            var entitiesDict = entities.ToDictionary(e => e.Id);

            foreach (var form in formsList)
            {
                if (entitiesDict.TryGetValue(form.Id, out var entity))
                {
                    _mapper.Map(form, entity);
                }
            }

            await _repository.UpdateRangeAsync(entities);

            var updatedModels = _mapper.Map<IEnumerable<IrregularFormModel>>(entities);

            _logger.LogInformation("Оновлено рейтинг для {Count} неправильних форм", formsList.Count);
            return updatedModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні рейтингу неправильних форм");
            throw;
        }
    }
}
