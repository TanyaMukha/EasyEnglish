namespace MukhaLab.Database;

/// <summary>
/// <see cref="IUserContext"/> implementation for applications without per-user authorization.
/// Always reports <see cref="Guid.Empty"/> as the current user.
/// </summary>
/// <remarks>
/// Registering this does not, by itself, disable per-user data scoping in
/// <see cref="BaseRepository{T, TContext}"/> — it still makes a repository's user filtering active
/// (any non-null <see cref="IUserContext"/> does), so if
/// <see cref="BaseRepository{T, TContext}.ConfigureUserIdField"/> is also called, every query will
/// be scoped to rows whose user-id property equals <see cref="Guid.Empty"/>. To genuinely disable
/// per-user scoping for a repository, simply never call <c>ConfigureUserIdField</c> on it (or
/// construct it with <c>userContext: null</c>).
/// </remarks>
public class AnonymousUserContext : IUserContext
{
    /// <inheritdoc/>
    public Guid GetCurrentUserId()
    {
        return Guid.Empty;
    }
}
