using MukhaLab.Database;
using System.Text.Json.Serialization;

namespace EasyPeasy.Core.Models;

/// <summary>DTO for <see cref="EasyPeasy.Core.Entities.ExampleEntity"/>.</summary>
public class ExampleModel : AbstractModel, IGuidRecord
{
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    public string Sentence { get; set; } = string.Empty;

    public string? Translation { get; set; }

    public int WordId { get; set; }

    [JsonIgnore]
    public WordModel? Word { get; set; }
}
