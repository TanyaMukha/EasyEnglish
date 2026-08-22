using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Models;
using EasyPeasy.Core.Interfaces.Fields;
using EasyPeasy.Core.Interfaces.Services;
using EasyPeasy.Core.Interfaces.Repositories;
using EasyPeasy.Core.Options;
using EasyPeasy.Core.Presets;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MukhaLab.Database;

namespace EasyPeasy.Business.Services;

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
    /// <b>Any <c>Id</c> on <paramref name="incoming"/>'s children is ignored and overwritten</b> — the
    /// graph typically arrives from a course archive produced by a different app instance, where those
    /// IDs refer to entirely unrelated rows. RecordGuid is the only identity that travels.
    /// </para>
    /// <para>
    /// A <c>null</c> child collection on <paramref name="incoming"/> (or a matched word's
    /// <c>Examples</c>) means "don't touch this collection" — it's replaced with the unit's existing
    /// children before reconciliation runs, so it's never treated as emptied out. To actually delete
    /// every child of a kind, pass an explicit empty list with <see cref="UnitMergeOptions.DeleteMissing"/>
    /// <c>true</c>.
    /// </para>
    /// <para>
    /// <paramref name="options"/> covers the case where the incoming graph is only <em>partially</em>
    /// authoritative: <see cref="UnitMergeOptions.MergeExamples"/> <c>false</c> leaves stored examples
    /// alone entirely, and <see cref="UnitMergeOptions.LearningProgress"/> decides whether incoming
    /// progress may overwrite what's stored. Both exist because an archive exported without that data
    /// still deserializes to *default* values, which would otherwise silently wipe real data.
    /// </para>
    /// </remarks>
    /// <exception cref="EntityNotFoundException"><paramref name="incoming"/>.Id doesn't match an existing unit.</exception>
    public async Task<UnitModel> ReconcileAndUpdateAsync(UnitModel incoming, UnitMergeOptions options, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(incoming.Id, UnitIncludes.Full, cancellationToken)
            ?? throw new EntityNotFoundException($"Unit with id {incoming.Id} was not found");

        // A null collection means "leave these children alone" -- fill it in from the current state
        // before any reconciliation/orphan logic runs, so it's never mistaken for "explicitly emptied."
        incoming.Words ??= existing.Words;
        incoming.IrregularForms ??= existing.IrregularForms;
        incoming.StudyCards ??= existing.StudyCards;
        incoming.TestCards ??= existing.TestCards;

        MergeProgress(incoming, existing, options.LearningProgress);

        var existingWordsByGuid = (existing.Words ?? []).ToDictionary(w => w.RecordGuid);
        var orphanExampleIds = new List<int>();

        ReconcileChildren(incoming.Words, existing.Words ?? [], options.LearningProgress);
        foreach (var word in incoming.Words ?? [])
        {
            if (!existingWordsByGuid.TryGetValue(word.RecordGuid, out var existingWord))
                continue; // new word -- its examples are new too, nothing to reconcile against

            // Without MergeExamples the payload says nothing about examples, so whatever it carries
            // (typically an empty list from an export that excluded them) must not be applied.
            if (!options.MergeExamples)
            {
                word.Examples = existingWord.Examples;
                continue;
            }

            word.Examples ??= existingWord.Examples;
            ReconcileIds(word.Examples, existingWord.Examples ?? []);
            if (options.DeleteMissing)
                orphanExampleIds.AddRange(FindOrphanIds(word.Examples, existingWord.Examples ?? []));
        }

        ReconcileChildren(incoming.IrregularForms, existing.IrregularForms ?? [], options.LearningProgress);
        ReconcileChildren(incoming.StudyCards, existing.StudyCards ?? [], options.LearningProgress);
        ReconcileChildren(incoming.TestCards, existing.TestCards ?? [], options.LearningProgress);

        if (options.DeleteMissing)
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
    /// Rewrites every item's <c>Id</c> from its <c>RecordGuid</c>: a match in <paramref name="existing"/>
    /// gets that row's real <c>Id</c> (updated in place), anything else is forced to <c>0</c> so it's
    /// inserted as a new row. <b>Whatever <c>Id</c> the caller supplied is discarded either way</b> — the
    /// graph usually comes from another app instance where those IDs point at unrelated rows, and an
    /// unzeroed foreign Id would make EF either collide with or silently overwrite a stranger's row.
    /// A no-op when <paramref name="incoming"/> is <c>null</c>.
    /// </summary>
    private static void ReconcileIds<T>(IList<T>? incoming, IEnumerable<T> existing)
        where T : AbstractModel, IGuidRecord
    {
        if (incoming is null) return;

        var existingIdsByGuid = existing.ToDictionary(e => e.RecordGuid, e => e.Id);
        foreach (var item in incoming)
            item.Id = existingIdsByGuid.TryGetValue(item.RecordGuid, out var existingId) ? existingId : 0;
    }

    /// <summary>
    /// <see cref="ReconcileIds"/> plus per-item learning-progress merging, for the child types that
    /// track review state. Both run off the same RecordGuid match, so they're done in one pass.
    /// </summary>
    private static void ReconcileChildren<T>(IList<T>? incoming, IEnumerable<T> existing, LearningProgressMerge policy)
        where T : AbstractModel, IGuidRecord, IReviewInfo
    {
        if (incoming is null) return;

        var existingByGuid = existing.ToDictionary(e => e.RecordGuid);
        foreach (var item in incoming)
        {
            if (!existingByGuid.TryGetValue(item.RecordGuid, out var match))
            {
                item.Id = 0;
                continue;
            }

            item.Id = match.Id;
            MergeProgress(item, match, policy);
        }
    }

    /// <summary>
    /// Copies <paramref name="existing"/>'s learning progress onto <paramref name="incoming"/> when the
    /// stored side should win: always under <see cref="LearningProgressMerge.KeepExisting"/>, and under
    /// <see cref="LearningProgressMerge.PreferNewest"/> only when it was reviewed more recently (a
    /// never-reviewed item counts as oldest). Otherwise the incoming values stand.
    /// <c>Rate</c> travels with the review state rather than being merged separately — a rating without
    /// the review history that produced it would be meaningless.
    /// </summary>
    private static void MergeProgress(IReviewInfo incoming, IReviewInfo existing, LearningProgressMerge policy)
    {
        var existingWins = policy == LearningProgressMerge.KeepExisting
            || (existing.LastReviewDate ?? DateTime.MinValue) > (incoming.LastReviewDate ?? DateTime.MinValue);

        if (!existingWins) return;

        incoming.LastReviewDate = existing.LastReviewDate;
        incoming.ReviewCount = existing.ReviewCount;

        if (incoming is IRateInfo incomingRate && existing is IRateInfo existingRate)
            incomingRate.Rate = existingRate.Rate;
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