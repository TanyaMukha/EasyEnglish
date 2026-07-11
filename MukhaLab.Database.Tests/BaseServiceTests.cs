using MukhaLab.Database.Tests.Fixtures;

namespace MukhaLab.Database.Tests;

public class BaseServiceTests : SqliteTestBase
{
    [Fact]
    public async Task CreateAsync_MapsAndPersistsEntity()
    {
        var service = CreateService();

        var model = await service.CreateAsync(new TestModel { Title = "Apple", Quantity = 5 });

        Assert.True(model.Id > 0);
        var found = await service.GetByIdAsync(model.Id);
        Assert.NotNull(found);
        Assert.Equal("Apple", found!.Title);
        Assert.Equal(5, found.Quantity);
    }

    [Fact]
    public async Task UpdateAsync_MapsChangesOntoExistingEntity()
    {
        var service = CreateService();
        var created = await service.CreateAsync(new TestModel { Title = "Apple", Quantity = 5 });

        created.Title = "Updated Apple";
        var updated = await service.UpdateAsync(created.Id, created);

        Assert.Equal("Updated Apple", updated.Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntity()
    {
        var service = CreateService();
        var created = await service.CreateAsync(new TestModel { Title = "Apple" });

        var result = await service.DeleteAsync(created.Id);

        Assert.True(result);
        Assert.Null(await service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task CreateRangeAsync_PersistsAllModels()
    {
        var service = CreateService();

        var models = await service.CreateRangeAsync(
        [
            new TestModel { Title = "Apple" },
            new TestModel { Title = "Banana" }
        ]);

        Assert.Equal(2, models.Count());
        Assert.Equal(2, (await service.GetAllAsync()).Count());
    }

    [Fact]
    public async Task UpdateRangeAsync_UpdatesAllRequestedEntities()
    {
        var service = CreateService();
        var a = await service.CreateAsync(new TestModel { Title = "Apple" });
        var b = await service.CreateAsync(new TestModel { Title = "Banana" });

        a.Title = "Apple2";
        b.Title = "Banana2";
        var updated = await service.UpdateRangeAsync([(a.Id, a), (b.Id, b)]);

        Assert.Equal(["Apple2", "Banana2"], updated.Select(m => m.Title).OrderBy(t => t));
    }

    [Fact]
    public async Task DeleteRangeAsync_DeletesAllRequestedEntities()
    {
        var service = CreateService();
        var a = await service.CreateAsync(new TestModel { Title = "Apple" });
        var b = await service.CreateAsync(new TestModel { Title = "Banana" });

        var result = await service.DeleteRangeAsync([a.Id, b.Id]);

        Assert.True(result);
        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task CountAsync_ReturnsMappedEntityCount()
    {
        var service = CreateService();
        await service.CreateAsync(new TestModel { Title = "Apple" });
        await service.CreateAsync(new TestModel { Title = "Banana" });

        var count = await service.CountAsync();

        Assert.Equal(2, count);
    }
}
