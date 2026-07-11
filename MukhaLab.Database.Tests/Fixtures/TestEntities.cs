using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace MukhaLab.Database.Tests.Fixtures;

public class TestEntity : AbstractEntity
{
    public string Title { get; set; } = string.Empty;
    public int Quantity { get; set; }

    /// <summary>Scalar owner-id column, used by the direct (non-collection) per-user scoping tests.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Child rows, used by the collection-navigation per-user scoping tests (the <c>Any()</c> branch of <c>IncludeUserIdFilter</c>).</summary>
    public List<TestOwnerLink> OwnerLinks { get; set; } = new();
}

/// <summary>Child row with its own owner id, for testing collection-path user-id scoping (e.g. "OwnerLinks.OwnerId").</summary>
public class TestOwnerLink
{
    public int Id { get; set; }
    public Guid OwnerId { get; set; }
    public int TestEntityId { get; set; }
    public TestEntity? TestEntity { get; set; }
}

public class TestGuidEntity : AbstractEntity, IGuidRecord
{
    public Guid RecordGuid { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
}

public class TestModel : AbstractModel
{
    public string Title { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    public DbSet<TestOwnerLink> TestOwnerLinks => Set<TestOwnerLink>();
    public DbSet<TestGuidEntity> TestGuidEntities => Set<TestGuidEntity>();
}
