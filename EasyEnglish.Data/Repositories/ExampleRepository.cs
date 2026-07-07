using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

public class ExampleRepository : BaseRepository<ExampleEntity, EasyEnglishDbContext>, IExampleRepository
{
    public ExampleRepository(IDbContextFactory<EasyEnglishDbContext> contextFactory, IUserContext userContext)
        : base(contextFactory, userContext)
    {
    }

    public async Task<List<ExampleEntity>> GetByUnitAsync(int unitId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Examples
            .AsNoTracking()
            .Where(e => e.Word!.UnitId == unitId)
            .ToListAsync();
    }
}
