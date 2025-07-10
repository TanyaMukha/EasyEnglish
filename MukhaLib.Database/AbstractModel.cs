namespace MukhaLib.Database;

/// <summary>
/// Base abstract model class for all business logic layer models.
/// Contains shared properties such as Id, CreatedAt, and UpdatedAt.
/// </summary>
public abstract class AbstractModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractModel"/> class with a specific ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    protected AbstractModel(int id)
    {
        this.Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractModel"/> class with a default ID of 0.
    /// </summary>
    protected AbstractModel()
    {
        this.Id = 0;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the model.
    /// </summary>
    public int Id { get; set; }
}
