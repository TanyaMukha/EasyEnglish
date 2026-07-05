using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Models;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Services.Services;

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

    public async Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId)
    {
        try
        {
            _logger.LogDebug("Отримання ID сусідніх навчальних карток для картки {CardId} у модулі {UnitId}", currentCardId, unitId);
            return await _studyCardRepository.GetNavigationIdsAsync(unitId, currentCardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні ID сусідніх навчальних карток для картки {CardId}", currentCardId);
            throw;
        }
    }
}
