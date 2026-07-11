namespace MukhaLab.Database;

/// <summary>
/// Thrown when an entity lookup, or an operation that requires an existing and (if per-user scoping
/// is active) currently-owned entity — update, delete, batch update, batch delete — fails to find a
/// matching, visible row. Used consistently by <see cref="BaseRepository{T, TContext}"/> and
/// <see cref="BaseService{TEntity, TModel}"/> in place of the previously mixed use of
/// <see cref="InvalidOperationException"/>/<see cref="ArgumentException"/> for "not found" conditions.
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>Initializes a new instance with no message.</summary>
    public EntityNotFoundException()
    {
    }

    /// <summary>Initializes a new instance with the specified error message.</summary>
    /// <param name="message">The message that describes the error.</param>
    public EntityNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with the specified error message and inner exception.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public EntityNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
