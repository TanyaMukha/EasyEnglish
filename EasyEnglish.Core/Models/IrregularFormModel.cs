using MukhaLab.Database;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using EasyEnglish.Core.Interfaces.Fields;
using System.Text.Json.Serialization;

namespace EasyEnglish.Core.Models;

/// <summary>DTO for <see cref="EasyEnglish.Core.Entities.IrregularFormEntity"/>.</summary>
public class IrregularFormModel : AbstractModel, IReviewInfo, IRateInfo, IAuditInfo, IGuidRecord
{
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    public string FirstForm { get; set; } = string.Empty;

    public string? PartOfSpeech { get; set; }

    public string? FirstFormTranscription { get; set; }

    public string? FirstFormTranslation { get; set; }

    public byte[]? FirstFormPronunciation { get; set; }

    public string SecondForm { get; set; } = string.Empty;

    public string? SecondFormTranscription { get; set; }

    public string? SecondFormTranslation { get; set; }

    public byte[]? SecondFormPronunciation { get; set; }

    public string? ThirdForm { get; set; }

    public string? ThirdFormTranscription { get; set; }

    public string? ThirdFormTranslation { get; set; }

    public byte[]? ThirdFormPronunciation { get; set; }

    public DateTime? LastReviewDate { get; set; }

    public int ReviewCount { get; set; } = 0;

    public float Rate { get; set; } = 3;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int UnitId { get; set; }

    [JsonIgnore]
    public UnitModel? Unit { get; set; }
}
