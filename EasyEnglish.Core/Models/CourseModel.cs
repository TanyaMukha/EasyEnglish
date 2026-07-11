using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;

namespace EasyEnglish.Core.Models;

/// <summary>DTO for <see cref="EasyEnglish.Core.Entities.CourseEntity"/>.</summary>
public class CourseModel : AbstractModel, IGuidInfo, IAuditInfo, IGuidRecord
{
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>The course's language, as a BCP-47 tag (e.g. "en-us").</summary>
    public string? LanguageCode { get; set; } = "en-us";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int? SubjectId { get; set; }

    public SubjectModel? Subject { get; set; }

    public ICollection<UnitModel>? Units { get; set; }
}
