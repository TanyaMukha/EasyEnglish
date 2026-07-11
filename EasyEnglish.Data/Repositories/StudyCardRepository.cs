using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Options;
using EasyEnglish.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

/// <summary>EF Core-backed <see cref="IStudyCardRepository"/>.</summary>
public class StudyCardRepository : BaseRepository<StudyCardEntity, EasyEnglishDbContext>, IStudyCardRepository
{
    public StudyCardRepository(IDbContextFactory<EasyEnglishDbContext> contextFactory, IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    /// <inheritdoc/>
    public async Task<List<StudyCardEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        IQueryable<StudyCardEntity> query = ctx.StudyCards
            .Where(c => c.Unit!.CourseId == courseId);

        if (unitId is not null)
            query = query.Where(c => c.UnitId == unitId);

        return await query.AsNoTracking().ApplyLearningSelectionAsync(options);
    }

    /// <inheritdoc/>
    /// <remarks>Same cyclic-navigation approach as <see cref="WordRepository.GetNavigationIdsAsync"/>.</remarks>
    public async Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.StudyCards
            .Where(c => c.UnitId == unitId)
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .GetCyclicNavigationAsync(currentCardId);
    }

    /// <inheritdoc/>
    public async Task<int> CountReviewedSinceAsync(DateTime since)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.StudyCards.CountAsync(c => c.LastReviewDate >= since);
    }
}
