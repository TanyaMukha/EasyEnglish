namespace MukhaLab.Database;

/// <summary>
/// Interface for the base service that provides common CRUD operations for an entity.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IBaseWithGuidRepository<T> : IBaseRepository<T>
    where T : class, IGuidRecord
{
    Task<T?> FindAsync(Guid guid);

    Task<IEnumerable<Guid>> CheckExistingGuidsAsync(IEnumerable<Guid> guids);
}
