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

    /// <summary>How many items to select. Default: 15.</summary>
    public int WordCount { get; set; }

    /// <summary>Selection strategy. Default: <see cref="LearningPriority.Random"/>.</summary>
    public LearningPriority Priority { get; set; }

    /// <summary>Whether already-learned items are eligible for selection. Default: <c>false</c>.</summary>
    public bool IncludeLearnedWords { get; set; }

    /// <summary>Whether to shuffle the selected items' order. Default: <c>false</c>.</summary>
    public bool ShuffleWords { get; set; }
}
