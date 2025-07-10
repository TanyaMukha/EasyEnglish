using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MukhaLib.Database;

/// <summary>
/// Abstract base class for all entities. Contains common properties like Id, CreatedAt, and UpdatedAt.
/// </summary>
public abstract class AbstractEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractEntity"/> class with a specified ID.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected AbstractEntity(int id)
    {
        this.Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractEntity"/> class with default ID (0).
    /// </summary>
    protected AbstractEntity()
    {
        this.Id = 0;
    }

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }
}
