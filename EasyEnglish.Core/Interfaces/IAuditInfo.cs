namespace EasyEnglish.Core.Interfaces;

public interface IAuditInfo
{
    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the update timestamp, if the entity has been modified.
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}
