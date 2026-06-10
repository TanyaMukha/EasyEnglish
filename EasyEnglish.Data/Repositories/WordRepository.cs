using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

public class WordRepository : BaseRepository<WordEntity, EasyEnglishDbContext>, IWordRepository
{
    public WordRepository(
        IMapper mapper,
        IDbContextFactory<EasyEnglishDbContext> contextFactory,
        IUserContext? userContext = null)
        : base(mapper, contextFactory, userContext)
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
}
