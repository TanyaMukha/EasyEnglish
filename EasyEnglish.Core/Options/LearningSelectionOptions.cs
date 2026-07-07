using EasyEnglish.Core.Enums;

namespace EasyEnglish.Core.Options;

/// <summary>Options controlling how items (words, irregular forms, cards) are selected for a learning session.</summary>
public class LearningSelectionOptions
{
    public LearningSelectionOptions()
    {
        WordCount = 15;
        Priority = LearningPriority.Random;
        IncludeLearnedWords = false;
        ShuffleWords = false;
    }

    public int WordCount { get; set; }
    public LearningPriority Priority { get; set; }
    public bool IncludeLearnedWords { get; set; }
    public bool ShuffleWords { get; set; }
}
