using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Options;
using EasyEnglish.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

public class WordRepository : BaseRepository<WordEntity, EasyEnglishDbContext>, IWordRepository
{
    public WordRepository(
        IDbContextFactory<EasyEnglishDbContext> contextFactory,
        IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

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

        // Циклічна навігація: з останнього слова - на перше, з першого - на останнє.
        var previousId = wordIds.Count > 1
            ? wordIds[(currentIndex - 1 + wordIds.Count) % wordIds.Count]
            : (int?)null;
        var nextId = wordIds.Count > 1
            ? wordIds[(currentIndex + 1) % wordIds.Count]
            : (int?)null;

        return (previousId, nextId, currentIndex + 1, wordIds.Count);
    }

    public async Task<List<WordEntity>> GetNextWordsAsync(int count)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Words
            .AsNoTracking()
            .OrderBy(w => w.LastReviewDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<WordEntity>> GetHardWordsAsync(int count)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Words
            .AsNoTracking()
            .OrderByDescending(w => w.Rate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<WordEntity>> GetByUnitAsync(int unitId, string[]? includes = null)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        IQueryable<WordEntity> query = ctx.Words.Where(w => w.UnitId == unitId);

        if (includes is not null)
            foreach (var include in includes)
                query = query.Include(include);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<List<WordEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        IQueryable<WordEntity> query = ctx.Words
            .Where(w => w.Unit!.CourseId == courseId);

        if (unitId is not null)
            query = query.Where(w => w.UnitId == unitId);

        return await query.AsNoTracking().ApplyLearningSelectionAsync(options);
    }

    public async Task<int> CountReviewedSinceAsync(DateTime since)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Words.CountAsync(w => w.LastReviewDate >= since);
    }
}
