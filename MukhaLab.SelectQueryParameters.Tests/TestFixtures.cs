namespace MukhaLab.SelectQueryParameters.Tests;

public class TestAuthor
{
    public string? Name { get; set; }
}

public class TestChild
{
    public string? Title { get; set; }
    public TestAuthor? Author { get; set; }
}

public class TestParent
{
    public List<TestChild> Children { get; set; } = new();
}

public class TestItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid ExternalId { get; set; }
    public Guid? ArchivedId { get; set; }
    public TestAuthor? Author { get; set; }

    // Always non-null: plain dot-path navigation (e.g. "Parent.Children[Title]") never null-checks
    // an intermediate step, so a null Parent would throw NullReferenceException under
    // LINQ-to-Objects even though the same path translates to a null-safe SQL join under a real
    // provider like EF Core. Every item gets a Parent (with a possibly-empty Children list) so
    // collection-path tests exercise real matching, not this unrelated LINQ-to-Objects limitation.
    public TestParent Parent { get; set; } = new();

    public List<TestChild> Children { get; set; } = new();
}

/// <summary>
/// Fixed sample data reused across test classes. Each test gets a fresh list so tests can't
/// interfere with each other by mutating shared state.
/// </summary>
public static class TestData
{
    public static readonly Guid AppleExternalId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BananaExternalId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CherryExternalId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid DateExternalId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid BananaArchivedId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static List<TestItem> SampleItems() =>
    [
        new TestItem
        {
            Id = 1,
            Title = "Apple",
            Quantity = 10,
            Price = 1.50m,
            CreatedAt = new DateTime(2024, 1, 10),
            ReviewedAt = null,
            IsActive = true,
            ExternalId = AppleExternalId,
            ArchivedId = null,
            Author = null,
            // Every TestChild here gets a non-null Author: "Children[Author.Name]" tests exist to
            // verify path *parsing* (a dot inside brackets), not cross-provider null-propagation
            // differences — plain multi-hop navigation never null-checks intermediate steps under
            // LINQ-to-Objects (unlike a real SQL provider's NULL semantics), so a null Author here
            // would throw for an unrelated reason.
            Children = [new TestChild { Title = "John", Author = new TestAuthor { Name = "Alice" } }]
        },
        new TestItem
        {
            Id = 2,
            Title = "Banana",
            Quantity = 5,
            Price = 0.75m,
            CreatedAt = new DateTime(2024, 2, 15),
            ReviewedAt = new DateTime(2024, 3, 1),
            IsActive = false,
            ExternalId = BananaExternalId,
            ArchivedId = BananaArchivedId,
            Author = null,
            Children = [new TestChild { Title = "Bob", Author = new TestAuthor { Name = "Nina" } }]
        },
        new TestItem
        {
            Id = 3,
            Title = "Cherry",
            Quantity = 20,
            Price = 3.00m,
            CreatedAt = new DateTime(2024, 1, 20),
            ReviewedAt = null,
            IsActive = true,
            ExternalId = CherryExternalId,
            ArchivedId = null,
            Author = null,
            Children = []
        },
        new TestItem
        {
            Id = 4,
            Title = "Date",
            Quantity = 0,
            Price = 5.25m,
            CreatedAt = new DateTime(2024, 3, 5),
            ReviewedAt = new DateTime(2024, 3, 10),
            IsActive = true,
            ExternalId = DateExternalId,
            ArchivedId = null,
            Author = new TestAuthor { Name = "Zoe" },
            Children = [],
            Parent = new TestParent { Children = [new TestChild { Title = "Carol", Author = new TestAuthor { Name = "Omar" } }] }
        },
    ];
}
