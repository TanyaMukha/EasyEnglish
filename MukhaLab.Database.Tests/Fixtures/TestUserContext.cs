using MukhaLab.Database;

namespace MukhaLab.Database.Tests.Fixtures;

/// <summary>Settable <see cref="IUserContext"/> so a test can control "the current user".</summary>
public class TestUserContext : IUserContext
{
    public Guid CurrentUserId { get; set; }

    public Guid GetCurrentUserId() => CurrentUserId;
}
