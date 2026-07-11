using Microsoft.EntityFrameworkCore;

namespace MukhaLab.Database;

/// <summary>
/// <see cref="BaseRepository{T, TContext}"/> extended with <see cref="IGuidRecord.RecordGuid"/>-based
/// lookups, for entities that need a stable identity independent of the auto-incrementing <c>int</c>
/// primary key (e.g. reconciling records across re-imports).
/// </summary>
/// <typeparam name="T">The type of the entity. Must derive from <see cref="AbstractEntity"/> and implement <see cref="IGuidRecord"/>.</typeparam>
/// <typeparam name="TContext">The type of the DbContext.</typeparam>
public class BaseWithGuidRepository<T, TContext> : BaseRepository<T, TContext>, IBaseWithGuidRepository<T>
    where T : AbstractEntity, IGuidRecord
    where TContext : DbContext
{
    /// <inheritdoc cref="BaseRepository{T, TContext}(IDbContextFactory{TContext}, IUserContext?)"/>
    public BaseWithGuidRepository(
        IDbContextFactory<TContext> contextFactory,
        IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    /// <inheritdoc/>
    public async Task<T?> FindAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await ctx.Set<T>().AsNoTracking().FirstOrDefaultAsync(c => c.RecordGuid == guid, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Guid>> CheckExistingGuidsAsync(IEnumerable<Guid> guids, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await ctx.Set<T>()
            .Where(u => guids.Contains(u.RecordGuid))
            .Select(u => u.RecordGuid)
            .ToListAsync(cancellationToken);
    }
}
