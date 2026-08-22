using AutoMapper;
using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Interfaces.Repositories;
using EasyPeasy.Core.Interfaces.Services;
using EasyPeasy.Core.Models;
using EasyPeasy.Core.Options;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyPeasy.Business.Services;

/// <summary>Service for <see cref="IrregularFormModel"/>, beyond the generic CRUD in <see cref="BaseService{T, TModel}"/>.</summary>
public class IrregularFormService : BaseService<IrregularFormEntity ,IrregularFormModel>, IIrregularFormService
{
    private readonly IIrregularFormRepository _irregularFormRepository;

    public IrregularFormService(
        IIrregularFormRepository repository,
        IMapper mapper,
        ILogger<IrregularFormService> logger)
        : base(repository, mapper, logger)
    {
        _irregularFormRepository = repository;
    }

    /// <inheritdoc/>
    /// <remarks>Shuffling happens here, after the repository query returns — not pushed down to SQL.</remarks>
    public async Task<IEnumerable<IrregularFormModel>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        var entities = await _irregularFormRepository.GetForLearningAsync(courseId, unitId, options);

        IEnumerable<IrregularFormEntity> result = entities;
        if (options.ShuffleWords)
            result = result.OrderBy(_ => Random.Shared.Next());

        return _mapper.Map<IEnumerable<IrregularFormModel>>(result);
    }

    /// <inheritdoc/>
    public Task<int> CountReviewedSinceAsync(DateTime since) => _irregularFormRepository.CountReviewedSinceAsync(since);

    /// <inheritdoc/>
    /// <remarks>Ids in <paramref name="forms"/> not found among existing rows are silently skipped, not reported.</remarks>
    public async Task<IEnumerable<IrregularFormModel>> UpdateRateRangeAsync(IEnumerable<UpdateWordRateRequest> forms)
    {
        var formsList = forms.ToList();
        _logger.LogDebug("Updating rating for {Count} irregular forms", formsList.Count);

        List<int> ids = formsList.Select(f => f.Id).ToList();

        // Fetch WITHOUT includes for the update
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

        _logger.LogInformation("Updated rating for {Count} irregular forms", formsList.Count);
        return updatedModels;
    }
}
