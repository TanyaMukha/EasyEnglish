namespace MukhaLab.Database;

/// <summary>
/// Supplies the identity of the currently active user to <see cref="BaseRepository{T, TContext}"/>
/// for row-level, per-user data scoping. Implement this against your app's actual authentication
/// state (e.g. a claims principal); for apps without per-user authorization, register
/// <see cref="AnonymousUserContext"/> instead — but see its remarks for an important caveat about
/// what that does and does not disable.
/// </summary>
public interface IUserContext
{
    /// <summary>Gets the identifier of the currently active user, used to scope queries via <see cref="BaseRepository{T, TContext}.ConfigureUserIdField"/>.</summary>
    Guid GetCurrentUserId();
}
