using EasyEnglish.Core.Enums;

namespace EasyEnglish.Core.Options;

public class WordSelectionOptions
{
    public WordSelectionOptions()
    {
        WordCount = 15;
        Priority = WordPriority.Random;
        IncludeLearnedWords = false;
        ShuffleWords = false;
    }

    public int WordCount { get; set; }
    public WordPriority Priority { get; set; }
    public bool IncludeLearnedWords { get; set; }
    public bool ShuffleWords { get; set; }
}
