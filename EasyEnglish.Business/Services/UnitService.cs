using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Presets;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyEnglish.Business.Services;

/// <summary>Service for <see cref="UnitModel"/>, beyond the generic CRUD in <see cref="BaseWithGuidService{T, TModel}"/>.</summary>
public class UnitService : BaseWithGuidService<UnitEntity, UnitModel>, IUnitService
{
    private readonly IUnitRepository unitRepository;
    private readonly IWordService wordService;
    private readonly IExampleService exampleService;
    private readonly IIrregularFormService irregularFormService;
    private readonly IStudyCardService studyCardService;
    private readonly ITestCardService testCardService;

    public UnitService(
        IUnitRepository repository,
        IMapper mapper,
        ILogger<UnitService> logger,
        IWordService wordService,
        IExampleService exampleService,
        IIrregularFormService irregularFormService,
        IStudyCardService studyCardService,
        ITestCardService testCardService)
        : base(repository, mapper, logger)
    {
        this.unitRepository = repository;
        this.wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
        this.exampleService = exampleService ?? throw new ArgumentNullException(nameof(exampleService));
        this.irregularFormService = irregularFormService ?? throw new ArgumentNullException(nameof(irregularFormService));
        this.studyCardService = studyCardService ?? throw new ArgumentNullException(nameof(studyCardService));
        this.testCardService = testCardService ?? throw new ArgumentNullException(nameof(testCardService));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UnitCardModel>> GetUnitCardsAsync(int courseId)
    {
        _logger.LogDebug("Loading unit cards for course {CourseId}", courseId);
        return await this.unitRepository.GetUnitCardsAsync(courseId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UnitModel>> GetByCourseAsync(int courseId)
    {
        var entities = await this.unitRepository.GetByCourseAsync(courseId);
        return _mapper.Map<IEnumerable<UnitModel>>(entities);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WordModel>> GetWordsAsync(int unitId, string[]? includes = null)
    {
        return await this.wordService.GetByUnitAsync(unitId, includes);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ExampleModel>> GetExamplesAsync(int unitId)
    {
        return await this.exampleService.GetByUnitAsync(unitId);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Updates a unit together with its children (Words/Examples/IrregularForms/StudyCards/TestCards),
    /// matching each child against the existing unit's children by <see cref="MukhaLab.Database.IGuidRecord.RecordGuid"/>
    /// instead of a blind EF cascade by Id. A child whose RecordGuid already exists among the current
    /// unit's children gets that row's real Id (and is updated in place); a new RecordGuid keeps Id at
    /// 0 (inserted as a new row). FKs (UnitId/WordId) don't need manual fixing — EF assigns them when
    /// saving the graph through the navigation collections.
    /// <para>
    /// A <c>null</c> child collection on <paramref name="incoming"/> (or a matched word's
    /// <c>Examples</c>) means "don't touch this collection" — it's replaced with the unit's existing
    /// children before reconciliation runs, so it's never treated as emptied out. To actually delete
    /// every child of a kind, pass an explicit empty list with <paramref name="deleteMissing"/> <c>true</c>.
    /// </para>
    /// </remarks>
    /// <param name="deleteMissing">
    /// When <c>true</c>, children whose RecordGuid isn't among the incoming ones are deleted from the
    /// database (strict sync). When <c>false</c>, such children are left unchanged (add/update only).
    /// </param>
    /// <exception cref="EntityNotFoundException"><paramref name="incoming"/>.Id doesn't match an existing unit.</exception>
    public async Task<UnitModel> ReconcileAndUpdateAsync(UnitModel incoming, bool deleteMissing, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(incoming.Id, UnitIncludes.Full, cancellationToken)
            ?? throw new EntityNotFoundException($"Unit with id {incoming.Id} was not found");

        // A null collection means "leave these children alone" -- fill it in from the current state
        // before any reconciliation/orphan logic runs, so it's never mistaken for "explicitly emptied."
        incoming.Words ??= existing.Words;
        incoming.IrregularForms ??= existing.IrregularForms;
        incoming.StudyCards ??= existing.StudyCards;
        incoming.TestCards ??= existing.TestCards;

        var existingWordsByGuid = (existing.Words ?? []).ToDictionary(w => w.RecordGuid);
        var orphanExampleIds = new List<int>();

        ReconcileIds(incoming.Words, existing.Words ?? []);
        foreach (var word in incoming.Words ?? [])
        {
            if (!existingWordsByGuid.TryGetValue(word.RecordGuid, out var existingWord))
                continue; // new word -- its examples are new too, nothing to reconcile against

            word.Examples ??= existingWord.Examples;
            ReconcileIds(word.Examples, existingWord.Examples ?? []);
            if (deleteMissing)
                orphanExampleIds.AddRange(FindOrphanIds(word.Examples, existingWord.Examples ?? []));
        }

        ReconcileIds(incoming.IrregularForms, existing.IrregularForms ?? []);
        ReconcileIds(incoming.StudyCards, existing.StudyCards ?? []);
        ReconcileIds(incoming.TestCards, existing.TestCards ?? []);

        if (deleteMissing)
        {
            await DeleteOrphansAsync(wordService, FindOrphanIds(incoming.Words, existing.Words ?? []), cancellationToken);
            await DeleteOrphansAsync(exampleService, orphanExampleIds, cancellationToken);
            await DeleteOrphansAsync(irregularFormService, FindOrphanIds(incoming.IrregularForms, existing.IrregularForms ?? []), cancellationToken);
            await DeleteOrphansAsync(studyCardService, FindOrphanIds(incoming.StudyCards, existing.StudyCards ?? []), cancellationToken);
            await DeleteOrphansAsync(testCardService, FindOrphanIds(incoming.TestCards, existing.TestCards ?? []), cancellationToken);
        }

        return await UpdateAsync(incoming.Id, incoming, cancellationToken);
    }

    /// <summary>
    /// For each item in <paramref name="incoming"/> whose <c>RecordGuid</c> matches an item in
    /// <paramref name="existing"/>, overwrites its <c>Id</c> with that existing row's real <c>Id</c> —
    /// in place. Items with no match keep <c>Id == 0</c> (a plain new <see cref="AbstractModel"/>
    /// default), so they'll be inserted as new rows. A no-op when <paramref name="incoming"/> is <c>null</c>.
    /// </summary>
    private static void ReconcileIds<T>(IList<T>? incoming, IEnumerable<T> existing)
        where T : AbstractModel, IGuidRecord
    {
        if (incoming is null) return;

        var existingIdsByGuid = existing.ToDictionary(e => e.RecordGuid, e => e.Id);
        foreach (var item in incoming)
            if (existingIdsByGuid.TryGetValue(item.RecordGuid, out var existingId))
                item.Id = existingId;
    }

    /// <summary>
    /// Returns the <c>Id</c>s of every item in <paramref name="existing"/> whose <c>RecordGuid</c> is
    /// not present in <paramref name="incoming"/> — i.e. children that were removed from the incoming
    /// graph and should be deleted under strict sync. A <c>null</c> <paramref name="incoming"/> is
    /// treated the same as an empty collection, so <em>every</em> existing item comes back as an orphan.
    /// </summary>
    private static List<int> FindOrphanIds<T>(IList<T>? incoming, IEnumerable<T> existing)
        where T : AbstractModel, IGuidRecord
    {
        var incomingGuids = (incoming ?? []).Select(i => i.RecordGuid).ToHashSet();
        return existing.Where(e => !incomingGuids.Contains(e.RecordGuid)).Select(e => e.Id).ToList();
    }

    /// <summary>Deletes <paramref name="orphanIds"/> via <paramref name="service"/>, if there are any.</summary>
    private static async Task DeleteOrphansAsync<TModel>(IBaseService<TModel> service, List<int> orphanIds, CancellationToken cancellationToken)
        where TModel : class
    {
        if (orphanIds.Count > 0)
            await service.DeleteRangeAsync(orphanIds, cancellationToken);
    }
}