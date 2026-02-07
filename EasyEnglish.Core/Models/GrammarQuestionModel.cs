using MukhaLab.Database;

namespace EasyEnglish.Core.Models;

public class GrammarQuestionModel : AbstractModel
{
    public string Question { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public IEnumerable<string> Options { get; set; } = new List<string>();
    public string? Explanation { get; set; }
    public int Order { get; set; }
}