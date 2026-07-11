using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Options;
using EasyEnglish.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

/// <summary>EF Core-backed <see cref="IWordRepository"/>.</summary>
public class WordRepository : BaseRepository<WordEntity, EasyEnglishDbContext>, IWordRepository
{
    public WordRepository(
        IDbContextFactory<EasyEnglishDbContext> contextFactory,
        IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Loads every word id in the unit and does the position/neighbor math in memory rather than in
    /// SQL — simplest way to express cyclic wraparound (last → first, first → last). Fine at this
    /// app's scale (a unit's word count), not written to scale to very large units.
    /// </remarks>
    public async Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentWordId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var wordIds = await ctx.Words
            .Where(w => w.UnitId == unitId)
            .OrderBy(w => w.Id)
            .Select(w => w.Id)
            .ToListAsync();

        var currentIndex = wordIds.IndexOf(currentWordId);
        if (currentIndex == -1)
            return (null, null, 0, wordIds.Count);

        // Cyclic navigation: last word wraps to the first, first word wraps to the last.
        var previousId = wordIds.Count > 1
            ? wordIds[(currentIndex - 1 + wordIds.Count) % wordIds.Count]
            : (int?)null;
        var nextId = wordIds.Count > 1
            ? wordIds[(currentIndex + 1) % wordIds.Count]
            : (int?)null;

        return (previousId, nextId, currentIndex + 1, wordIds.Count);
    }

    /// <inheritdoc/>
    public async Task<List<WordEntity>> GetNextWordsAsync(int count)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Words
            .AsNoTracking()
            .OrderBy(w => w.LastReviewDate)
            .Take(count)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<WordEntity>> GetHardWordsAsync(int count)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Words
            .AsNoTracking()
            .OrderByDescending(w => w.Rate)
            .Take(count)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<WordEntity>> GetByUnitAsync(int unitId, string[]? includes = null)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        IQueryable<WordEntity> query = ctx.Words.Where(w => w.UnitId == unitId);

        if (includes is not null)
            foreach (var include in includes)
                query = query.Include(include);

        return await query.AsNoTracking().ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<WordEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        IQueryable<WordEntity> query = ctx.Words
            .Where(w => w.Unit!.CourseId == courseId);

        if (unitId is not null)
            query = query.Where(w => w.UnitId == unitId);

        return await query.AsNoTracking().ApplyLearningSelectionAsync(options);
    }

    /// <inheritdoc/>
    public async Task<int> CountReviewedSinceAsync(DateTime since)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Words.CountAsync(w => w.LastReviewDate >= since);
    }
}
