using EasyEnglish.App.Services;
using EasyEnglish.Core.Models;

namespace EasyEnglish.App.Models;

public static class LanguageLevels
{
    public static readonly Dictionary<string, string> LevelDictionary = new()
    {
        { "A1", "Beginner" },
        { "A2", "Elementary" },
        { "B1", "Intermediate" },
        { "B2", "Upper-Intermediate" },
        { "C1", "Advanced" },
        { "C2", "Proficiency" }
    };

    public static IEnumerable<(string Level, string Description)> GetAll()
    {
        return LevelDictionary.Select(kvp => (kvp.Key, kvp.Value));
    }

    public static string GetDescription(string level)
    {
        return LevelDictionary.TryGetValue(level, out var description) ? description : level;
    }
}

public class DailyProgress
{
    public Dictionary<string, int> CompletedTests { get; set; } = new();
    public Dictionary<string, int> IncorrectTests { get; set; } = new();
}

public class ProgressStats
{
    public DailyProgress DailyProgress { get; set; } = new();
    public int TotalProgress { get; set; }
}

public class StreakInfo
{
    public int CurrentStreak { get; set; }
    public int HighestStreak { get; set; }
    public DateTime? LastVisitDate { get; set; }
}

public class DailyGoals
{
    public Dictionary<string, DailyGoal> Goals { get; set; } = new();
}

public class DailyGoal
{
    public bool Enabled { get; set; }
    public int Count { get; set; }
}

public class WordTestModel : WordModel
{
    public int LastTotalAttempts { get; set; } = 0;
    public int LastIncorrectAttempts { get; set; } = 0;

    public TestModel Tests { get; set; } = new();

    // Обчислювані властивості для UI
    public float CurrentRating { get; set; } = 3; // Розраховується на льоту
    public bool NeedsReview { get; set; } = false; // Чи потребує повторення
    public DateTime? NextReviewDate { get; set; } // Рекомендована дата наступного повторення
    public int? DaysSinceLastReview { get; set; } 
}

public class TestModel
{
    public TestDetailModel WordToTranslation { get; set; } = new();
    public TestDetailModel TranslationToWord { get; set; } = new();
    public TestDetailModel DefinitionToWord { get; set; } = new();

    // Індексатор для доступу через CardDirection
    public TestDetailModel this[CardDirection direction]
    {
        get => direction switch
        {
            CardDirection.WordToTranslation => WordToTranslation,
            CardDirection.TranslationToWord => TranslationToWord,
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
        set
        {
            switch (direction)
            {
                case CardDirection.WordToTranslation:
                    WordToTranslation = value;
                    break;
                case CardDirection.TranslationToWord:
                    TranslationToWord = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }
    }
}

public class TestDetailModel
{
    public TestResult KnowOrNot { get; set; } = new();
    public TestResult ManualInput { get; set; } = new();
    public TestResult SingleChoice { get; set; } = new();
    public TestResult MultipleChoice { get; set; } = new();
    public TestResult Matching { get; set; } = new();

    // Індексатор для доступу через CardType
    public TestResult this[CardType type]
    {
        get => type switch
        {
            CardType.KnowOrNot => KnowOrNot,
            CardType.ManualInput => ManualInput,
            CardType.SingleChoice => SingleChoice,
            CardType.MultipleChoice => MultipleChoice,
            CardType.Matching => Matching,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        set
        {
            switch (type)
            {
                case CardType.KnowOrNot:
                    KnowOrNot = value;
                    break;
                case CardType.ManualInput:
                    ManualInput = value;
                    break;
                case CardType.SingleChoice:
                    SingleChoice = value;
                    break;
                case CardType.MultipleChoice:
                    MultipleChoice = value;
                    break;
                case CardType.Matching:
                    Matching = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}

public class TestResult
{
    public int TotalAttempts { get; set; } = 0;
    public int CorrectAnswers { get; set; } = 0;
}

public class WordsForTodayResult
{
    public List<WordModel> Words { get; set; } = new();
    public UnitModel? Unit { get; set; }
}

public class WordIdsForToday
{
    public List<int> WordIds { get; set; } = new();
    public int? UnitId { get; set; }
}

public class OptionDefinition
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
