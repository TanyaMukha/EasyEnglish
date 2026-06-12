using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Options;
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

    public async Task<(int? PreviousId, int? NextId)> GetNavigationIdsAsync(int unitId, int currentWordId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var wordIds = await ctx.Words
            .Where(w => w.UnitId == unitId)
            .OrderBy(w => w.Id)
            .Select(w => w.Id)
            .ToListAsync();

        var currentIndex = wordIds.IndexOf(currentWordId);
        if (currentIndex == -1)
            return (null, null);

        var previousId = currentIndex > 0 ? wordIds[currentIndex - 1] : (int?)null;
        var nextId = currentIndex < wordIds.Count - 1 ? wordIds[currentIndex + 1] : (int?)null;

        return (previousId, nextId);
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

    public async Task<List<WordEntity>> GetByUnitAsync(int unitId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Words
            .AsNoTracking()
            .Where(w => w.UnitId == unitId)
            .ToListAsync();
    }

    public async Task<List<WordEntity>> GetForLearningAsync(int courseId, int? unitId, WordSelectionOptions options)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        IQueryable<WordEntity> query = ctx.Words
            .Where(w => w.Unit!.CourseId == courseId);

        if (unitId is not null)
            query = query.Where(w => w.UnitId == unitId);

        if (options.IncludeLearnedWords)
            query = query.Where(w => w.Rate >= 1.6f);

        query = options.Priority switch
        {
            WordPriority.New => query
                .Where(w => w.LastReviewDate == null)
                .OrderByDescending(w => w.CreatedAt),

            WordPriority.Random => query
                .OrderBy(w => w.CreatedAt)
                .ThenBy(w => w.LastReviewDate),

            WordPriority.Difficult => query
                .OrderByDescending(w => w.Rate),

            WordPriority.Review => query
                .Where(w => w.LastReviewDate != null)
                .OrderBy(w => w.LastReviewDate),

            WordPriority.Old => query
                .Where(w => w.LastReviewDate == null)
                .OrderBy(w => w.CreatedAt),

            _ => query
        };

        if (options.WordCount > 0)
            query = query.Take(options.WordCount);

        return await query.AsNoTracking().ToListAsync();
    }
}
