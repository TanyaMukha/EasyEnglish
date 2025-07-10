using System.Diagnostics.CodeAnalysis;

namespace MukhaLib.Database;

public abstract class AbstractMainModel : AbstractModel
{
    /// <summary>
    /// Gets or sets the identifier of the entity.
    /// </summary>
    [NotNull]
    public Guid RecordGuid { get; set; } = System.Guid.NewGuid(); // Automatically generate a new GUID for each instance
}
