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
        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);
            return await set.FirstOrDefaultAsync(c => c.RecordGuid == guid);
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    public async Task<IEnumerable<Guid>> CheckExistingGuidsAsync(IEnumerable<Guid> guids)
    {
        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);
            return await set
                .Where(u => guids.Contains(u.RecordGuid))
                .Select(u => u.RecordGuid)
                .ToListAsync();
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }
}