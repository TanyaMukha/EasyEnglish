using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;
using MukhaLab.Database.Tests.Fixtures;

namespace MukhaLab.Database.Tests;

/// <summary>
/// Regression tests: a row deleted out-of-band (simulating a concurrent delete from another
/// operation) causes UpdateAsync/UpdateRangeAsync on a stale reference to throw
/// EntityNotFoundException (wrapping DbUpdateConcurrencyException) instead of an unhandled
/// exception or a silent no-op success.
/// </summary>
public class ConcurrencyTests : SqliteTestBase
{
    [Fact]
    public async Task UpdateAsync_ConcurrentlyDeletedRow_ThrowsEntityNotFoundExceptionWrappingConcurrencyException()
    {
        var repository = CreateRepository();
        var entity = await repository.AddAsync(new TestEntity { Title = "Apple" });

        await DeleteOutOfBandAsync(entity.Id);

        entity.Title = "Updated";
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.UpdateAsync(entity));
        Assert.IsType<DbUpdateConcurrencyException>(exception.InnerException);
    }

    [Fact]
    public async Task UpdateRangeAsync_ConcurrentlyDeletedRow_ThrowsEntityNotFoundExceptionWrappingConcurrencyException()
    {
        var repository = CreateRepository();
        var kept = await repository.AddAsync(new TestEntity { Title = "Kept" });
        var deleted = await repository.AddAsync(new TestEntity { Title = "WillBeDeleted" });

        await DeleteOutOfBandAsync(deleted.Id);

        kept.Title = "Kept-Updated";
        deleted.Title = "Deleted-Updated";

        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.UpdateRangeAsync([kept, deleted]));
        Assert.IsType<DbUpdateConcurrencyException>(exception.InnerException);
    }

    private async Task DeleteOutOfBandAsync(int id)
    {
        await using var ctx = await Factory.CreateDbContextAsync();
        var entity = await ctx.TestEntities.FindAsync(id);
        ctx.TestEntities.Remove(entity!);
        await ctx.SaveChangesAsync();
    }
}
