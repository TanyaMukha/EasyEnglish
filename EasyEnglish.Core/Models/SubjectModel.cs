using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;

namespace EasyEnglish.Core.Models;

/// <summary>DTO for <see cref="EasyEnglish.Core.Entities.SubjectEntity"/>.</summary>
public class SubjectModel : AbstractModel, IAuditInfo
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
