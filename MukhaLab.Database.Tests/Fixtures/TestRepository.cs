using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace MukhaLab.Database.Tests.Fixtures;

public class TestRepository : BaseRepository<TestEntity, TestDbContext>
{
    public TestRepository(IDbContextFactory<TestDbContext> contextFactory, IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    /// <summary>
    /// Adds <paramref name="entity"/> and, in the same transaction, a linked <see cref="TestOwnerLink"/>
    /// child row — exercising the protected ctx-scoped <see cref="BaseRepository{T, TContext}.Add(TContext, T)"/>
    /// helper together with a raw <c>DbSet</c> call inside <see cref="BaseRepository{T, TContext}.ExecuteInTransactionAsync{TResult}"/>.
    /// </summary>
    public Task<TestEntity> ImportWithLinkAsync(TestEntity entity, TestOwnerLink link, bool throwBeforeCommit = false)
    {
        return ExecuteInTransactionAsync(async ctx =>
        {
            Add(ctx, entity);
            await ctx.SaveChangesAsync();

            if (throwBeforeCommit)
                throw new InvalidOperationException("Simulated failure before commit.");

            link.TestEntityId = entity.Id;
            ctx.Set<TestOwnerLink>().Add(link);
            await ctx.SaveChangesAsync();

            return entity;
        });
    }
}
