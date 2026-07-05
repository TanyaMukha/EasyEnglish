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

/// <summary>
/// Курований список мов курсу для форми створення/редагування курсу.
/// Код — BCP-47 (використовується і для прапора, і збережеться в LanguageCode).
/// </summary>
public static class CourseLanguages
{
    public static readonly IReadOnlyList<(string Code, string Label)> All =
    [
        ("en-us", "Англійська (США)"),
        ("en-gb", "Англійська (Великобританія)"),
        ("uk-ua", "Українська"),
        ("de-de", "Німецька"),
        ("fr-fr", "Французька"),
        ("es-es", "Іспанська"),
        ("it-it", "Італійська"),
        ("pl-pl", "Польська"),
        ("pt-pt", "Португальська"),
        ("tr-tr", "Турецька"),
    ];
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

/// <summary>
/// Модель, що накопичує результати тест-сесії та має рейтинг.
/// Реалізується тестовими моделями слів і неправильних дієслів —
/// WordRatingCalculator працює з будь-якою з них однаково.
/// </summary>
public interface ITestSessionItem
{
    float Rate { get; set; }
    DateTime? LastReviewDate { get; set; }
    int ReviewCount { get; set; }
    DateTime? UpdatedAt { get; set; }
    int LastTotalAttempts { get; set; }
    int LastIncorrectAttempts { get; set; }
    TestModel Tests { get; }
}

public static class TestSessionItemExtensions
{
    /// <summary>Записує одну відповідь у статистику сесії.</summary>
    public static void RecordTestAnswer(
        this ITestSessionItem item, CardDirection direction, CardType type, bool isCorrect)
    {
        var result = item.Tests[direction][type];
        result.TotalAttempts++;
        if (isCorrect)
            result.CorrectAnswers++;
    }

    /// <summary>Фіксує перегляд картки (review-тести без відповідей).</summary>
    public static void RecordReview(this ITestSessionItem item)
    {
        item.LastReviewDate = DateTime.UtcNow;
        item.ReviewCount++;
    }
}

public class WordTestModel : WordModel, ITestSessionItem
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

public class IrregularFormTestModel : IrregularFormModel, ITestSessionItem
{
    public int LastTotalAttempts { get; set; } = 0;
    public int LastIncorrectAttempts { get; set; } = 0;

    public TestModel Tests { get; set; } = new();
}

/// <summary>
/// Item для мульті-тесту прикладів. Рейтинг ведеться по слову,
/// якому належить приклад, — через посилання TestWord.
/// </summary>
public class ExampleTestModel : ExampleModel
{
    public WordTestModel? TestWord { get; set; }
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

/// <summary>
/// Налаштування видимості полів для швидкої форми додавання слів.
/// Поле "Слово" завжди відображається і сюди не входить.
/// </summary>
public class WordShortFormFieldSettings
{
    public bool ShowTranscription { get; set; } = true;
    public bool ShowTranslation { get; set; } = true;
    public bool ShowExample { get; set; }
    public bool ShowExampleTranslation { get; set; }
}
