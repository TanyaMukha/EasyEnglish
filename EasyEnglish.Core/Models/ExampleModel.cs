using MukhaLab.Database;
using System.Text.Json.Serialization;

namespace EasyEnglish.Core.Models;

public class ExampleModel : AbstractModel
{
    public string Sentence { get; set; } = string.Empty;

    public string? Translation { get; set; }

    public int WordId { get; set; }

    [JsonIgnore]
    public WordModel? Word { get; set; }
}
