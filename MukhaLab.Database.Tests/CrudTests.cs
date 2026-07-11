using MukhaLab.Database;
using MukhaLab.Database.Tests.Fixtures;

namespace MukhaLab.Database.Tests;

public class CrudTests : SqliteTestBase
{
    [Fact]
    public async Task AddAsync_PersistsEntity()
    {
        var repository = CreateRepository();

        var added = await repository.AddAsync(new TestEntity { Title = "Apple", Quantity = 10 });

        Assert.True(added.Id > 0);
        var found = await repository.FindAsync(added.Id);
        Assert.NotNull(found);
        Assert.Equal("Apple", found!.Title);
    }

    [Fact]
    public async Task GetAsync_ReturnsAllEntities()
    {
        var repository = CreateRepository();
        await repository.AddAsync(new TestEntity { Title = "Apple" });
        await repository.AddAsync(new TestEntity { Title = "Banana" });

        var result = (await repository.GetAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task FindAsync_MissingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var result = await repository.FindAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task FindManyAsync_ReturnsOnlyExistingIds()
    {
        var repository = CreateRepository();
        var a = await repository.AddAsync(new TestEntity { Title = "Apple" });
        var b = await repository.AddAsync(new TestEntity { Title = "Banana" });

        var result = await repository.FindManyAsync([a.Id, b.Id, 999]);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var repository = CreateRepository();
        var entity = await repository.AddAsync(new TestEntity { Title = "Apple", Quantity = 1 });

        entity.Quantity = 42;
        await repository.UpdateAsync(entity);

        var found = await repository.FindAsync(entity.Id);
        Assert.Equal(42, found!.Quantity);
    }

    [Fact]
    public async Task RemoveAsync_DeletesEntity()
    {
        var repository = CreateRepository();
        var entity = await repository.AddAsync(new TestEntity { Title = "Apple" });

        var result = await repository.RemoveAsync(entity.Id);

        Assert.True(result);
        Assert.Null(await repository.FindAsync(entity.Id));
    }

    [Fact]
    public async Task RemoveRangeAsync_ByIds_DeletesAll()
    {
        var repository = CreateRepository();
        var a = await repository.AddAsync(new TestEntity { Title = "Apple" });
        var b = await repository.AddAsync(new TestEntity { Title = "Banana" });

        var result = await repository.RemoveRangeAsync([a.Id, b.Id]);

        Assert.True(result);
        Assert.Empty(await repository.GetAsync());
    }

    [Fact]
    public async Task RemoveRangeAsync_ByEntities_ReachableThroughInterface_DeletesAll()
    {
        var repository = CreateRepository();
        var a = await repository.AddAsync(new TestEntity { Title = "Apple" });
        var b = await repository.AddAsync(new TestEntity { Title = "Banana" });

        // Regression: RemoveRangeAsync(IEnumerable<T>) must be reachable through the interface,
        // not just the concrete class.
        IBaseRepository<TestEntity> asInterface = repository;
        var result = await asInterface.RemoveRangeAsync([a, b]);

        Assert.True(result);
        Assert.Empty(await repository.GetAsync());
    }

    [Fact]
    public async Task RemoveRangeAsync_ByKeyValuesList_DeletesAll()
    {
        var repository = CreateRepository();
        var a = await repository.AddAsync(new TestEntity { Title = "Apple" });
        var b = await repository.AddAsync(new TestEntity { Title = "Banana" });

        var result = await repository.RemoveRangeAsync([new object[] { a.Id }, new object[] { b.Id }]);

        Assert.True(result);
        Assert.Empty(await repository.GetAsync());
    }

    [Fact]
    public async Task CountAsync_ReturnsTotalRowCount()
    {
        var repository = CreateRepository();
        await repository.AddAsync(new TestEntity { Title = "Apple" });
        await repository.AddAsync(new TestEntity { Title = "Banana" });

        var count = await repository.CountAsync();

        Assert.Equal(2, count);
    }
}
