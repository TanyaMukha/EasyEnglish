using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace MukhaLab.Database;

public class BaseWithGuidRepository<T, TContext> : BaseRepository<T, TContext>, IBaseWithGuidRepository<T>
    where T : class, IGuidRecord
    where TContext : DbContext
{
    public BaseWithGuidRepository(
        IMapper mapper,
        IDbContextFactory<TContext> contextFactory,
        IUserContext? userContext = null)
        : base(mapper, contextFactory, userContext)
    {
    }

    public async Task<T?> FindAsync(Guid guid)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Set<T>().AsNoTracking().FirstOrDefaultAsync(c => c.RecordGuid == guid);
    }

    public async Task<IEnumerable<Guid>> CheckExistingGuidsAsync(IEnumerable<Guid> guids)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Set<T>()
            .Where(u => guids.Contains(u.RecordGuid))
            .Select(u => u.RecordGuid)
            .ToListAsync();
    }
}
