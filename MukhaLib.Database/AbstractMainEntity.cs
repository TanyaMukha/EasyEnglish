using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MukhaLib.Database;

public abstract class AbstractMainEntity : AbstractEntity
{
    /// <summary>
    /// Gets or sets the identifier of the entity.
    /// </summary>
    [Required]
    [Column("guid")]
    public Guid RecordGuid { get; set; } = System.Guid.NewGuid(); // Automatically generate a new GUID for each instance
}
