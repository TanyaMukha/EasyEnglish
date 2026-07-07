using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Options;
using EasyEnglish.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

public class TestCardRepository : BaseRepository<TestCardEntity, EasyEnglishDbContext>, ITestCardRepository
{
    public TestCardRepository(IDbContextFactory<EasyEnglishDbContext> contextFactory, IUserContext userContext)
        : base(contextFactory, userContext)
    {
    }

    public async Task<List<TestCardEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        IQueryable<TestCardEntity> query = ctx.TestCards
            .Where(c => c.Unit!.CourseId == courseId);

        if (unitId is not null)
            query = query.Where(c => c.UnitId == unitId);

        query = query.ApplyLearningSelection(options);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<(int? PreviousId, int? NextId, int Position, int Total)> GetNavigationIdsAsync(int unitId, int currentCardId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var cardIds = await ctx.TestCards
            .Where(c => c.UnitId == unitId)
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .ToListAsync();

        var currentIndex = cardIds.IndexOf(currentCardId);
        if (currentIndex == -1)
            return (null, null, 0, cardIds.Count);

        var previousId = cardIds.Count > 1
            ? cardIds[(currentIndex - 1 + cardIds.Count) % cardIds.Count]
            : (int?)null;
        var nextId = cardIds.Count > 1
            ? cardIds[(currentIndex + 1) % cardIds.Count]
            : (int?)null;

        return (previousId, nextId, currentIndex + 1, cardIds.Count);
    }
}
