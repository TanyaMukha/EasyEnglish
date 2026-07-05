using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;

namespace EasyEnglish.Core.Models;

/// <summary>
/// Расширенная модель словаря с дополнительными вычисляемыми свойствами
/// </summary>
public partial class CourseModel : AbstractModel, IGuidInfo, IAuditInfo, IGuidRecord
{
    public Guid RecordGuid { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Мова курсу у форматі BCP-47 (наприклад, "en-us").
    /// </summary>
    public string? LanguageCode { get; set; } = "en-us";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    public ICollection<UnitModel>? Units { get; set; }

    //// ===== COMPUTED PROPERTIES =====

    ///// <summary>
    ///// Количество слов в словаре
    ///// </summary>
    //public int WordCount => Words?.Count ?? 0;

    ///// <summary>
    ///// Средний рейтинг слов в словаре
    ///// </summary>
    //public float Rate => Words?.Any() == true ? Words.Average(w => w.Rate) : 0f;

    ///// <summary>
    ///// Общее количество повторений всех слов
    ///// </summary>
    //public int TotalReviewCount => Words?.Sum(w => w.ReviewCount) ?? 0;

    ///// <summary>
    ///// Дата последнего изучения любого слова в словаре
    ///// </summary>
    //public DateTime? TotalLastReviewDate => Words?
    //    .Where(w => w.LastReviewDate.HasValue)
    //    .Max(w => w.LastReviewDate);

    ///// <summary>
    ///// Процент изученных слов (слова с рейтингом >= 4)
    ///// </summary>
    //public double StudiedWordsPercentage => WordCount > 0
    //    ? (Words?.Count(w => w.Rate >= 4.0f) ?? 0) * 100.0 / WordCount
    //    : 0.0;

    ///// <summary>
    ///// Количество слов по уровням сложности
    ///// </summary>
    //public Dictionary<string, int> WordsByLevel => Words?.GroupBy(w => w.Level)
    //    .ToDictionary(g => g.Key, g => g.Count()) ?? new Dictionary<string, int>();

    ///// <summary>
    ///// Количество слов по частям речи
    ///// </summary>
    //public Dictionary<string, int> WordsByPartOfSpeech => Words?.GroupBy(w => w.PartOfSpeech)
    //    .ToDictionary(g => g.Key, g => g.Count()) ?? new Dictionary<string, int>();

    ///// <summary>
    ///// Статус словаря
    ///// </summary>
    //public DictionaryStatus Status
    //{
    //    get
    //    {
    //        if (WordCount == 0) return DictionaryStatus.Empty;
    //        if (StudiedWordsPercentage >= 90) return DictionaryStatus.Completed;
    //        if (StudiedWordsPercentage >= 50) return DictionaryStatus.InProgress;
    //        if (TotalLastReviewDate.HasValue) return DictionaryStatus.Started;
    //        return DictionaryStatus.New;
    //    }
    //}

    ///// <summary>
    ///// Цвет для отображения статуса
    ///// </summary>
    //public string StatusColor => Status switch
    //{
    //    DictionaryStatus.Empty => "#64748B",        // muted
    //    DictionaryStatus.New => "#3B82F6",          // info
    //    DictionaryStatus.Started => "#F59E0B",      // warning
    //    DictionaryStatus.InProgress => "#8B5CF6",   // purple
    //    DictionaryStatus.Completed => "#10B981",    // success
    //    _ => "#64748B"
    //};

    ///// <summary>
    ///// Текстовое описание статуса
    ///// </summary>
    //public string StatusText => Status switch
    //{
    //    DictionaryStatus.Empty => "Пустой",
    //    DictionaryStatus.New => "Новый",
    //    DictionaryStatus.Started => "Начат",
    //    DictionaryStatus.InProgress => "В процессе",
    //    DictionaryStatus.Completed => "Завершен",
    //    _ => "Неизвестно"
    //};

    ///// <summary>
    ///// Рекомендуемое действие для словаря
    ///// </summary>
    //public string RecommendedAction => Status switch
    //{
    //    DictionaryStatus.Empty => "Добавить слова",
    //    DictionaryStatus.New => "Начать изучение",
    //    DictionaryStatus.Started => "Продолжить изучение",
    //    DictionaryStatus.InProgress => "Повторить сложные слова",
    //    DictionaryStatus.Completed => "Поддерживать знания",
    //    _ => "Открыть словарь"
    //};

    ///// <summary>
    ///// Получить краткую статистику словаря
    ///// </summary>
    //public DictionaryStatistics GetStatistics()
    //{
    //    return new DictionaryStatistics
    //    {
    //        TotalWords = WordCount,
    //        StudiedWords = Words?.Count(w => w.Rate >= 4.0f) ?? 0,
    //        ReviewsCount = TotalReviewCount,
    //        AverageRating = Rate,
    //        LastReviewDate = TotalLastReviewDate,
    //        WordsByLevel = WordsByLevel,
    //        WordsByPartOfSpeech = WordsByPartOfSpeech,
    //        StudiedPercentage = StudiedWordsPercentage,
    //        Status = Status
    //    };
    //}

    ///// <summary>
    ///// Проверить, нужно ли повторение
    ///// </summary>
    //public bool NeedsReview()
    //{
    //    if (!TotalLastReviewDate.HasValue) return WordCount > 0;

    //    var daysSinceLastReview = (DateTime.UtcNow - TotalLastReviewDate.Value).TotalDays;

    //    return Status switch
    //    {
    //        DictionaryStatus.New => true,
    //        DictionaryStatus.Started => daysSinceLastReview > 1,
    //        DictionaryStatus.InProgress => daysSinceLastReview > 3,
    //        DictionaryStatus.Completed => daysSinceLastReview > 7,
    //        _ => false
    //    };
    //}

    ///// <summary>
    ///// Получить слова, которые нужно повторить
    ///// </summary>
    //public IEnumerable<WordModel> GetWordsForReview()
    //{
    //    if (Words == null) return Enumerable.Empty<WordModel>();

    //    var now = DateTime.UtcNow;

    //    return Words.Where(word =>
    //    {
    //        if (!word.LastReviewDate.HasValue) return true;

    //        var daysSinceReview = (now - word.LastReviewDate.Value).TotalDays;

    //        return word.Rate switch
    //        {
    //            < 2.0f => daysSinceReview >= 1,  // Сложные слова - каждый день
    //            < 3.0f => daysSinceReview >= 3,  // Средние слова - каждые 3 дня
    //            < 4.0f => daysSinceReview >= 7,  // Хорошие слова - каждую неделю
    //            _ => daysSinceReview >= 14        // Отличные слова - каждые 2 недели
    //        };
    //    }).OrderBy(w => w.Rate).ThenBy(w => w.LastReviewDate);
    //}

    ///// <summary>
    ///// Обновить статистику после изменения слов
    ///// </summary>
    //public void RefreshStatistics()
    //{
    //    UpdatedAt = DateTime.UtcNow;
    //    // Свойства пересчитаются автоматически благодаря computed properties
    //}
}

/// <summary>
/// Статусы словаря
/// </summary>
public enum DictionaryStatus
{
    /// <summary>
    /// Словарь пустой (нет слов)
    /// </summary>
    Empty,

    /// <summary>
    /// Новый словарь (есть слова, но не было изучения)
    /// </summary>
    New,

    /// <summary>
    /// Изучение начато (< 50% изучено)
    /// </summary>
    Started,

    /// <summary>
    /// В процессе изучения (50-90% изучено)
    /// </summary>
    InProgress,

    /// <summary>
    /// Изучение завершено (> 90% изучено)
    /// </summary>
    Completed
}

/// <summary>
/// Статистика словаря
/// </summary>
public class DictionaryStatistics
{
    public int TotalWords { get; set; }
    public int StudiedWords { get; set; }
    public int ReviewsCount { get; set; }
    public float AverageRating { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public Dictionary<string, int> WordsByLevel { get; set; } = new();
    public Dictionary<string, int> WordsByPartOfSpeech { get; set; } = new();
    public double StudiedPercentage { get; set; }
    public DictionaryStatus Status { get; set; }

    /// <summary>
    /// Получить следующую цель для словаря
    /// </summary>
    public string GetNextGoal()
    {
        return Status switch
        {
            DictionaryStatus.Empty => $"Добавить первые слова",
            DictionaryStatus.New => $"Изучить {Math.Min(10, TotalWords)} слов",
            DictionaryStatus.Started => $"Довести до 50% ({TotalWords / 2} слов)",
            DictionaryStatus.InProgress => $"Довести до 90% ({(int)(TotalWords * 0.9)} слов)",
            DictionaryStatus.Completed => "Поддерживать знания",
            _ => "Продолжить изучение"
        };
    }

    /// <summary>
    /// Получить мотивирующее сообщение
    /// </summary>
    public string GetMotivationalMessage()
    {
        return Status switch
        {
            DictionaryStatus.Empty => "Начните с добавления новых слов!",
            DictionaryStatus.New => "Пора начать изучение этих слов!",
            DictionaryStatus.Started => $"Отлично! Уже {StudiedWords} слов изучено!",
            DictionaryStatus.InProgress => $"Великолепно! Осталось совсем немного!",
            DictionaryStatus.Completed => "Поздравляем! Словарь освоен!",
            _ => "Продолжайте в том же духе!"
        };
    }

    /// <summary>
    /// Получить рекомендацию по времени изучения
    /// </summary>
    public string GetTimeRecommendation()
    {
        if (TotalWords == 0) return "Добавьте слова для начала";

        var wordsPerDay = Status switch
        {
            DictionaryStatus.New => Math.Min(5, TotalWords),
            DictionaryStatus.Started => Math.Min(3, TotalWords - StudiedWords),
            DictionaryStatus.InProgress => Math.Min(2, TotalWords - StudiedWords),
            _ => 0
        };

        if (wordsPerDay == 0) return "Повторяйте изученные слова";

        var estimatedDays = (TotalWords - StudiedWords) / (double)wordsPerDay;

        return estimatedDays switch
        {
            <= 1 => "Можно завершить сегодня!",
            <= 7 => $"~{(int)Math.Ceiling(estimatedDays)} дней ({wordsPerDay} слов/день)",
            <= 30 => $"~{(int)Math.Ceiling(estimatedDays / 7)} недель ({wordsPerDay} слов/день)",
            _ => $"~{(int)Math.Ceiling(estimatedDays / 30)} месяцев ({wordsPerDay} слов/день)"
        };
    }
}