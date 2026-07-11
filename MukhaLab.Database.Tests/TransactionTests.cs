using MukhaLab.Database.Tests.Fixtures;

namespace MukhaLab.Database.Tests;

public class TransactionTests : SqliteTestBase
{
    [Fact]
    public async Task ExecuteInTransactionAsync_CommitsOnSuccess()
    {
        var repository = CreateRepository();

        var result = await repository.ExecuteInTransactionAsync(async ctx =>
        {
            ctx.TestEntities.Add(new TestEntity { Title = "Apple" });
            await ctx.SaveChangesAsync();
            return true;
        });

        Assert.True(result);
        Assert.Single(await repository.GetAsync());
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_RollsBackOnException()
    {
        var repository = CreateRepository();

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.ExecuteInTransactionAsync<object?>(async ctx =>
        {
            ctx.TestEntities.Add(new TestEntity { Title = "Apple" });
            await ctx.SaveChangesAsync();
            throw new InvalidOperationException("Simulated failure.");
        }));

        Assert.Empty(await repository.GetAsync());
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_VoidOverload_RollsBackOnException()
    {
        var repository = CreateRepository();

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.ExecuteInTransactionAsync(async ctx =>
        {
            ctx.TestEntities.Add(new TestEntity { Title = "Apple" });
            await ctx.SaveChangesAsync();
            throw new InvalidOperationException("Simulated failure.");
        }));

        Assert.Empty(await repository.GetAsync());
    }

    [Fact]
    public async Task CtxScopedAddHelper_ComposesWithRawDbSetCallsInsideTransaction()
    {
        var repository = CreateRepository();
        var entity = new TestEntity { Title = "Apple" };
        var link = new TestOwnerLink { OwnerId = Guid.NewGuid() };

        var result = await repository.ImportWithLinkAsync(entity, link);

        Assert.True(result.Id > 0);
        var found = await repository.FindAsync(result.Id);
        Assert.NotNull(found);

        await using var ctx = await Factory.CreateDbContextAsync();
        var persistedLink = Assert.Single(ctx.TestOwnerLinks.Where(l => l.TestEntityId == result.Id));
        Assert.Equal(link.OwnerId, persistedLink.OwnerId);
    }

    [Fact]
    public async Task CtxScopedAddHelper_RollsBackBothWritesOnException()
    {
        var repository = CreateRepository();
        var entity = new TestEntity { Title = "Apple" };
        var link = new TestOwnerLink { OwnerId = Guid.NewGuid() };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.ImportWithLinkAsync(entity, link, throwBeforeCommit: true));

        Assert.Empty(await repository.GetAsync());
    }
}
