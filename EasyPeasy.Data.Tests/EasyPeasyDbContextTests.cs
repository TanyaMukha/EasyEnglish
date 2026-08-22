using EasyPeasy.Core.Entities;
using EasyPeasy.Data.Tests.Fixtures;

namespace EasyPeasy.Data.Tests;

/// <summary>Tests for <see cref="EasyPeasyDbContext.SaveChangesAsync"/>'s automatic <c>CreatedAt</c>/<c>UpdatedAt</c> stamping.</summary>
public class EasyPeasyDbContextTests : SqliteTestBase
{
    [Fact]
    public async Task SaveChanges_NewEntity_SetsCreatedAt()
    {
        await using var ctx = CreateContext();
        var subject = new SubjectEntity { Title = "Subject" };
        ctx.Subjects.Add(subject);

        var before = DateTime.UtcNow;
        await ctx.SaveChangesAsync();
        var after = DateTime.UtcNow;

        Assert.InRange(subject.CreatedAt, before.AddSeconds(-1), after.AddSeconds(1));
        Assert.Null(subject.UpdatedAt);
    }

    [Fact]
    public async Task SaveChanges_ModifiedEntity_SetsUpdatedAtButLeavesCreatedAt()
    {
        SubjectEntity subject;
        await using (var ctx = CreateContext())
        {
            subject = new SubjectEntity { Title = "Subject" };
            ctx.Subjects.Add(subject);
            await ctx.SaveChangesAsync();
        }

        var originalCreatedAt = subject.CreatedAt;

        await using (var ctx = CreateContext())
        {
            var tracked = ctx.Subjects.Single();
            tracked.Title = "Renamed";
            await ctx.SaveChangesAsync();

            Assert.Equal(originalCreatedAt, tracked.CreatedAt);
            Assert.NotNull(tracked.UpdatedAt);
        }
    }
}
