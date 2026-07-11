using MukhaLab.Database;
using MukhaLab.Database.Tests.Fixtures;

namespace MukhaLab.Database.Tests;

/// <summary>
/// Regression tests for per-user row scoping: once <c>ConfigureUserIdField</c> is called, every
/// method — including by-key lookups, updates, and deletes, not just "query many" — must respect
/// ownership. Before the fix, only GetAsync/CountAsync applied the filter.
/// </summary>
public class PerUserScopingTests : SqliteTestBase
{
    private static readonly Guid CurrentUser = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OtherUser = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private TestRepository CreateScopedRepository(Guid currentUser)
    {
        var repository = CreateRepository(new TestUserContext { CurrentUserId = currentUser });
        repository.ConfigureUserIdField(["OwnerId"]);
        return repository;
    }

    [Fact]
    public async Task GetAsync_OnlyReturnsCurrentUsersRows()
    {
        var setup = CreateRepository();
        var mine = await setup.AddAsync(new TestEntity { Title = "Mine", OwnerId = CurrentUser });
        var theirs = await setup.AddAsync(new TestEntity { Title = "Theirs", OwnerId = OtherUser });

        var repository = CreateScopedRepository(CurrentUser);
        var result = (await repository.GetAsync()).ToList();

        var item = Assert.Single(result);
        Assert.Equal(mine.Id, item.Id);
        Assert.DoesNotContain(result, i => i.Id == theirs.Id);
    }

    [Fact]
    public async Task FindAsync_OnAnotherUsersRow_ReturnsNull()
    {
        var setup = CreateRepository();
        var theirs = await setup.AddAsync(new TestEntity { Title = "Theirs", OwnerId = OtherUser });

        var repository = CreateScopedRepository(CurrentUser);
        var result = await repository.FindAsync(theirs.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task FindAsync_OnOwnRow_ReturnsEntity()
    {
        var setup = CreateRepository();
        var mine = await setup.AddAsync(new TestEntity { Title = "Mine", OwnerId = CurrentUser });

        var repository = CreateScopedRepository(CurrentUser);
        var result = await repository.FindAsync(mine.Id);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task FindManyAsync_SilentlyOmitsAnotherUsersRows()
    {
        var setup = CreateRepository();
        var mine = await setup.AddAsync(new TestEntity { Title = "Mine", OwnerId = CurrentUser });
        var theirs = await setup.AddAsync(new TestEntity { Title = "Theirs", OwnerId = OtherUser });

        var repository = CreateScopedRepository(CurrentUser);
        var result = await repository.FindManyAsync([mine.Id, theirs.Id]);

        var item = Assert.Single(result);
        Assert.Equal(mine.Id, item.Id);
    }

    [Fact]
    public async Task UpdateAsync_OnAnotherUsersRow_ThrowsEntityNotFoundException()
    {
        var setup = CreateRepository();
        var theirs = await setup.AddAsync(new TestEntity { Title = "Theirs", OwnerId = OtherUser });
        theirs.Title = "Hijacked";

        var repository = CreateScopedRepository(CurrentUser);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.UpdateAsync(theirs));
    }

    [Fact]
    public async Task UpdateAsync_OnOwnRow_Succeeds()
    {
        var setup = CreateRepository();
        var mine = await setup.AddAsync(new TestEntity { Title = "Mine", OwnerId = CurrentUser });
        mine.Title = "Updated";

        var repository = CreateScopedRepository(CurrentUser);
        await repository.UpdateAsync(mine);

        var found = await setup.FindAsync(mine.Id);
        Assert.Equal("Updated", found!.Title);
    }

    [Fact]
    public async Task RemoveAsync_OnAnotherUsersRow_ThrowsEntityNotFoundException()
    {
        var setup = CreateRepository();
        var theirs = await setup.AddAsync(new TestEntity { Title = "Theirs", OwnerId = OtherUser });

        var repository = CreateScopedRepository(CurrentUser);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.RemoveAsync(theirs.Id));
    }

    [Fact]
    public async Task RemoveRangeAsync_ByIds_WithAnotherUsersRowIncluded_ThrowsAndDeletesNothing()
    {
        var setup = CreateRepository();
        var mine = await setup.AddAsync(new TestEntity { Title = "Mine", OwnerId = CurrentUser });
        var theirs = await setup.AddAsync(new TestEntity { Title = "Theirs", OwnerId = OtherUser });

        var repository = CreateScopedRepository(CurrentUser);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.RemoveRangeAsync([mine.Id, theirs.Id]));

        // All-or-nothing: since one id wasn't owned, neither row should have been deleted.
        Assert.NotNull(await setup.FindAsync(mine.Id));
        Assert.NotNull(await setup.FindAsync(theirs.Id));
    }

    [Fact]
    public async Task CollectionNavigationUserIdPath_FiltersCorrectly()
    {
        var setup = CreateRepository();
        var mine = await setup.AddAsync(new TestEntity { Title = "Mine" });
        var theirs = await setup.AddAsync(new TestEntity { Title = "Theirs" });

        await using (var ctx = await Factory.CreateDbContextAsync())
        {
            ctx.TestOwnerLinks.Add(new TestOwnerLink { TestEntityId = mine.Id, OwnerId = CurrentUser });
            ctx.TestOwnerLinks.Add(new TestOwnerLink { TestEntityId = theirs.Id, OwnerId = OtherUser });
            await ctx.SaveChangesAsync();
        }

        var repository = CreateRepository(new TestUserContext { CurrentUserId = CurrentUser });
        repository.ConfigureUserIdField(["OwnerLinks.OwnerId"]);

        var result = (await repository.GetAsync()).ToList();

        var item = Assert.Single(result);
        Assert.Equal(mine.Id, item.Id);
    }

    [Fact]
    public async Task WithoutConfigureUserIdField_NonNullUserContextDoesNotFilter()
    {
        // A non-null IUserContext alone does not activate scoping — ConfigureUserIdField must
        // also be called. This is the actual mechanism behind the AnonymousUserContext fix: simply
        // registering a non-null IUserContext (like AnonymousUserContext) is not, by itself,
        // enough to filter anything.
        var setup = CreateRepository();
        await setup.AddAsync(new TestEntity { Title = "Mine", OwnerId = CurrentUser });
        await setup.AddAsync(new TestEntity { Title = "Theirs", OwnerId = OtherUser });

        var repository = CreateRepository(new TestUserContext { CurrentUserId = CurrentUser });
        // Note: ConfigureUserIdField is never called here.

        var result = await repository.GetAsync();

        Assert.Equal(2, result.Count());
    }
}
