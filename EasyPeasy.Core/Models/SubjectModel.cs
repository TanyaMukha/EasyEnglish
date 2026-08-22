using EasyPeasy.Core.Interfaces.Fields;
using MukhaLab.Database;

namespace EasyPeasy.Core.Models;

/// <summary>DTO for <see cref="EasyPeasy.Core.Entities.SubjectEntity"/>.</summary>
public class SubjectModel : AbstractModel, IAuditInfo
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
