using MukhaLab.Database.Tests.Fixtures;

namespace MukhaLab.Database.Tests;

public class GuidRepositoryTests : SqliteTestBase
{
    [Fact]
    public async Task FindAsync_ByGuid_ReturnsMatchingEntity()
    {
        var repository = CreateGuidRepository();
        var entity = await repository.AddAsync(new TestGuidEntity { Title = "Apple" });

        var found = await repository.FindAsync(entity.RecordGuid);

        Assert.NotNull(found);
        Assert.Equal(entity.Id, found!.Id);
    }

    [Fact]
    public async Task FindAsync_ByUnknownGuid_ReturnsNull()
    {
        var repository = CreateGuidRepository();

        var found = await repository.FindAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task CheckExistingGuidsAsync_ReturnsOnlyExistingSubset()
    {
        var repository = CreateGuidRepository();
        var a = await repository.AddAsync(new TestGuidEntity { Title = "Apple" });
        var b = await repository.AddAsync(new TestGuidEntity { Title = "Banana" });
        var unknown = Guid.NewGuid();

        var existing = (await repository.CheckExistingGuidsAsync([a.RecordGuid, b.RecordGuid, unknown])).ToList();

        Assert.Equal(2, existing.Count);
        Assert.Contains(a.RecordGuid, existing);
        Assert.Contains(b.RecordGuid, existing);
        Assert.DoesNotContain(unknown, existing);
    }
}
