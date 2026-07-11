using MukhaLab.Database;
using MukhaLab.Database.Tests.Fixtures;

namespace MukhaLab.Database.Tests;

/// <summary>
/// Regression tests: every "not found" path across BaseRepository and BaseService throws
/// EntityNotFoundException specifically, not the previously mixed InvalidOperationException /
/// ArgumentException.
/// </summary>
public class NotFoundExceptionTests : SqliteTestBase
{
    [Fact]
    public async Task UpdateAsync_MissingEntity_ThrowsEntityNotFoundException()
    {
        var repository = CreateRepository();
        var ghost = new TestEntity { Id = 999, Title = "Ghost" };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.UpdateAsync(ghost));
    }

    [Fact]
    public async Task RemoveAsync_MissingEntity_ThrowsEntityNotFoundException()
    {
        var repository = CreateRepository();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.RemoveAsync(999));
    }

    [Fact]
    public async Task RemoveRangeAsync_ByIds_PartialMatch_ThrowsEntityNotFoundException()
    {
        var repository = CreateRepository();
        var entity = await repository.AddAsync(new TestEntity { Title = "Apple" });

        await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.RemoveRangeAsync([entity.Id, 999]));
    }

    [Fact]
    public async Task RemoveRangeAsync_ByKeyValuesList_MissingEntity_ThrowsEntityNotFoundException()
    {
        var repository = CreateRepository();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.RemoveRangeAsync([new object[] { 999 }]));
    }

    [Fact]
    public async Task ServiceUpdateAsync_MissingEntity_ThrowsEntityNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => service.UpdateAsync(999, new TestModel { Title = "Ghost" }));
    }

    [Fact]
    public async Task ServiceDeleteAsync_MissingEntity_ThrowsEntityNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => service.DeleteAsync(999));
    }

    [Fact]
    public async Task ServiceEntityNotFoundException_PropagatesFromRepository()
    {
        // The service layer should not swallow or rewrap EntityNotFoundException into a different type.
        var service = CreateService();

        var exception = await Record.ExceptionAsync(() => service.DeleteAsync(999));

        Assert.IsType<EntityNotFoundException>(exception);
    }
}
