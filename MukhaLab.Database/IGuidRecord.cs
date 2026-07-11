/// <summary>
/// Marks an entity/model as having a stable, client-generatable GUID identity
/// (<see cref="RecordGuid"/>), distinct from the database-generated <c>int</c> primary key used by
/// <see cref="AbstractEntity"/>/<see cref="AbstractModel"/>. Used by
/// <see cref="BaseWithGuidRepository{T, TContext}"/>/<see cref="BaseWithGuidService{TEntity, TModel}"/>
/// for GUID-based lookups (e.g. import/export reconciliation, where a durable identity is needed
/// across re-imports that the auto-incrementing <c>Id</c> cannot provide).
/// </summary>
/// <remarks>
/// This type is declared in the global namespace (no <c>namespace</c> statement), unlike every
/// other type in this library. It compiles and resolves correctly for existing consumers only
/// because global-namespace types are implicitly visible everywhere; it is not intentional and
/// should not be treated as a model to follow for new types.
/// </remarks>
public interface IGuidRecord
{
    /// <summary>Stable GUID identity for the record, independent of the database-generated <c>int</c> primary key.</summary>
    Guid RecordGuid { get; }
}