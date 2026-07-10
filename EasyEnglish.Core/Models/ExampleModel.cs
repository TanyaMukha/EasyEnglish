using MukhaLab.Database;
using System.Text.Json.Serialization;

namespace EasyEnglish.Core.Models;

public class ExampleModel : AbstractModel, IGuidRecord
{
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    public string Sentence { get; set; } = string.Empty;

    public string? Translation { get; set; }

    public int WordId { get; set; }

    [JsonIgnore]
    public WordModel? Word { get; set; }
}
