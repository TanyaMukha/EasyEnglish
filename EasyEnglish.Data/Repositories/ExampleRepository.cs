using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

/// <summary>EF Core-backed <see cref="IExampleRepository"/>.</summary>
public class ExampleRepository : BaseRepository<ExampleEntity, EasyEnglishDbContext>, IExampleRepository
{
    public ExampleRepository(IDbContextFactory<EasyEnglishDbContext> contextFactory, IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    /// <inheritdoc/>
    /// <remarks>Joins through <c>Example.Word.UnitId</c> — no direct <c>UnitId</c> column on <c>examples</c>.</remarks>
    public async Task<List<ExampleEntity>> GetByUnitAsync(int unitId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Examples
            .AsNoTracking()
            .Where(e => e.Word!.UnitId == unitId)
            .ToListAsync();
    }
}
