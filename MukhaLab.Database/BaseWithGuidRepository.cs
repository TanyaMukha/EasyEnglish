using Microsoft.EntityFrameworkCore;

namespace MukhaLab.Database;

public class BaseWithGuidRepository<T, TContext> : BaseRepository<T, TContext>, IBaseWithGuidRepository<T>
    where T : class, IGuidRecord
    where TContext : DbContext
{
    public BaseWithGuidRepository(
        IDbContextFactory<TContext> contextFactory,
        IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    public async Task<T?> FindAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await ctx.Set<T>().AsNoTracking().FirstOrDefaultAsync(c => c.RecordGuid == guid, cancellationToken);
    }

    public async Task<IEnumerable<Guid>> CheckExistingGuidsAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await ctx.Set<T>()
            .Where(u => guids.Contains(u.RecordGuid))
            .Select(u => u.RecordGuid)
            .ToListAsync(cancellationToken);
    }
}
