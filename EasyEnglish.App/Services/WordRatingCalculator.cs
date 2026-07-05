using EasyEnglish.App.Models;

namespace EasyEnglish.App.Services;

/// <summary>
/// Типи карток для повторення слів
/// </summary>
public enum CardType
{
    Review = -2,             // Повторення (використовується для позначення сесії повторення)
    QuickAnswer = -1,        // Швидка відповідь
    KnowOrNot = 0,           // Знаю/Не знаю (самооцінка)
    ManualInput = 1,         // Ввід вручну
    SingleChoice = 2,        // Вибір одного варіанта з декількох
    MultipleChoice = 3,      // Вибір кількох варіантів з декількох
    Matching = 4             // Сопоставлення варіантів (пари)
}

/// <summary>
/// Напрямок перекладу/питання в картці
/// </summary>
public enum CardDirection
{
    WordToTranslation = 0,        // Англійське слово → переклад
    TranslationToWord = 1,        // Переклад → англійське слово
}

public static class WordRatingCalculator
{
    private const float MIN_RATE = 0.0f;
    private const float MAX_RATE = 5.0f;
    private const float INITIAL_RATE = 3.0f;

    // ЗБАЛАНСОВАНІ коефіцієнти для плавної зміни складності (x1.5 для швидшої реакції рейтингу)
    private const float PERFECT_ANSWER_DECREASE = 0.45f;  // 95-100% (було 0.3f, до того 0.8f)
    private const float GOOD_ANSWER_DECREASE = 0.3f;      // 75-94% (було 0.2f, до того 0.5f)
    private const float AVERAGE_ANSWER_DECREASE = 0.15f;  // 65-74% (було 0.1f, до того 0.25f)
    private const float POOR_ANSWER_INCREASE = 0.225f;    // 50-64% (було 0.15f, до того 0.35f)
    private const float BAD_ANSWER_INCREASE = 0.375f;     // 25-49% (було 0.25f, до того 0.6f)
    private const float FAILED_ANSWER_INCREASE = 0.6f;    // 0-24% (було 0.4f, до того 1.0f)

    private const float EASY_WORD_MULTIPLIER = 0.7f;
    private const float HARD_WORD_MULTIPLIER = 1.5f;

    private const float FORGETTING_CURVE_COEFFICIENT = 0.05f;  // повернуто до помірного значення

    /// <summary>
    /// Ваги важливості для різних типів карток
    /// </summary>
    private static readonly Dictionary<CardType, float> _cardTypeWeights = new()
    {
        { CardType.SingleChoice, 0.5f },      // Середній вплив
        { CardType.KnowOrNot, 0.3f },         // МІЗЕРНИЙ вплив (було 0.7f)
        { CardType.ManualInput, 1.0f }        // НАЙВАГОМІШИЙ вплив
    };

    /// <summary>
    /// Ваги для різних напрямків
    /// </summary>
    private static readonly Dictionary<CardDirection, float> _directionWeights = new()
    {
        { CardDirection.WordToTranslation, 0.6f },     // Простіше, менший вплив (було 0.8f)
        { CardDirection.TranslationToWord, 1.0f },     // СКЛАДНІШЕ, більший вплив
    };

    /// <summary>
    /// Отримує доступні напрямки для кожного типу картки
    /// </summary>
    public static List<CardDirection> GetAvailableDirections(CardType cardType)
    {
        return cardType switch
        {
            CardType.SingleChoice => new List<CardDirection>
            {
                CardDirection.WordToTranslation,
                CardDirection.TranslationToWord
            },
            CardType.KnowOrNot => new List<CardDirection>
            {
                CardDirection.WordToTranslation,
                CardDirection.TranslationToWord
            },
            CardType.ManualInput => new List<CardDirection>
            {
                CardDirection.TranslationToWord
            },
            _ => new List<CardDirection>()
        };
    }

    /// <summary>
    /// Розраховує зміну рейтингу на основі успішності відповіді
    /// </summary>
    private static float CalculateRateChange(float successRate, float weight)
    {
        float baseChange = successRate switch
        {
            >= 0.95f => -PERFECT_ANSWER_DECREASE,
            >= 0.75f => -GOOD_ANSWER_DECREASE,
            >= 0.65f => -AVERAGE_ANSWER_DECREASE,
            >= 0.50f => POOR_ANSWER_INCREASE,
            >= 0.25f => BAD_ANSWER_INCREASE,
            _ => FAILED_ANSWER_INCREASE
        };

        return baseChange;
    }

    /// <summary>
    /// Розраховує поточний рейтинг з урахуванням часу, що минув з моменту останнього перегляду
    /// </summary>
    public static float CalculateCurrentRate<T>(
        T word,
        DateTime? referenceDate = null)
        where T : ITestSessionItem
    {
        DateTime now = referenceDate ?? DateTime.UtcNow;

        if (word.LastReviewDate == null)
            return word.Rate;

        TimeSpan timeSinceReview = now - word.LastReviewDate.Value;
        int daysSinceReview = (int)timeSinceReview.TotalDays;

        if (daysSinceReview <= 0)
            return word.Rate;

        float forgettingPenalty = CalculateForgettingPenalty(
            word.Rate,
            daysSinceReview,
            word.ReviewCount,
            word.LastTotalAttempts,
            word.LastIncorrectAttempts
        );

        float currentRating = Math.Clamp(word.Rate + forgettingPenalty, MIN_RATE, MAX_RATE);

        return currentRating;
    }

    public static List<T> UpdateWordRate<T>(List<T> items)
        where T : ITestSessionItem
    {
        try
        {
            var wordsToUpdate = new List<T>();

            // Обробляємо кожне слово окремо
            foreach (var word in items)
            {
                // Перевіряємо чи є дані для оновлення
                if (word.Tests == null)
                    continue;

                // Зберігаємо старий рейтинг для логування
                float oldRate = word.Rate;

                // Оновлюємо слово
                var updatedWord = WordRatingCalculator.UpdateWordAfterSession(word);

                wordsToUpdate.Add(updatedWord);
            }

            return wordsToUpdate;
        }
        catch (Exception ex)
        {
            return new List<T>();
        }
    }

    /// <summary>
    /// Оновлює рейтинг та статистику одного слова на основі результатів тестування
    /// </summary>
    /// <param name="word">Модель з результатами тестів сесії</param>
    /// <returns>Модель з оновленими даними для збереження в базу</returns>
    public static T UpdateWordAfterSession<T>(T word)
        where T : ITestSessionItem
    {
        // Перевіряємо чи є тести для цього слова
        if (word.Tests == null)
            return word;

        // Збираємо результати по всіх напрямках і типах
        int totalAttempts = 0;
        int totalCorrect = 0;
        float weightedRateChange = 0f;
        float totalWeight = 0f;

        foreach (CardDirection direction in Enum.GetValues<CardDirection>())
        {
            TestDetailModel? testDetail = null;

            try
            {
                testDetail = word.Tests[direction];
            }
            catch
            {
                continue;
            }

            if (testDetail == null)
                continue;

            foreach (CardType cardType in Enum.GetValues<CardType>())
            {
                TestResult? testResult = null;

                try
                {
                    testResult = testDetail[cardType];
                }
                catch
                {
                    continue;
                }

                if (testResult == null || testResult.TotalAttempts == 0)
                    continue;

                int sessionTotal = testResult.TotalAttempts;
                int sessionCorrect = testResult.CorrectAnswers;

                totalAttempts += sessionTotal;
                totalCorrect += sessionCorrect;

                float successRate = (float)sessionCorrect / sessionTotal;
                float cardWeight = _cardTypeWeights.GetValueOrDefault(cardType, 1.0f);
                float directionWeight = _directionWeights.GetValueOrDefault(direction, 1.0f);
                float weight = cardWeight * directionWeight;

                weightedRateChange += CalculateRateChange(successRate, weight) * weight;
                totalWeight += weight;
            }
        }

        // Якщо були спроби, оновлюємо рейтинг
        if (totalAttempts > 0 && totalWeight > 0)
        {
            float averageRateChange = weightedRateChange / totalWeight;

            // ПОМІРНИЙ модифікатор на основі кількості спроб
            float attemptModifier = 0.6f + Math.Min(totalAttempts / 12.0f, 0.25f);  // було 0.8f + totalAttempts / 8.0f
            float finalRateChange = averageRateChange * attemptModifier;

            word.Rate = Math.Clamp(word.Rate + finalRateChange, MIN_RATE, MAX_RATE);

            // Оновлюємо статистику
            word.LastReviewDate = DateTime.UtcNow;
            word.LastTotalAttempts = totalAttempts;
            word.LastIncorrectAttempts = totalAttempts - totalCorrect;
            word.ReviewCount++;
            word.UpdatedAt = DateTime.UtcNow;
        }

        return word;
    }

    private static float CalculateForgettingPenalty(
        float baseRate,
        int daysSinceReview,
        int reviewCount,
        int lastTotalAttempts,
        int lastIncorrectAttempts)
    {
        float lastSuccessRate = lastTotalAttempts > 0
            ? (float)(lastTotalAttempts - lastIncorrectAttempts) / lastTotalAttempts
            : 0.5f;

        float memoryStrengthFactor = CalculateMemoryStrength(reviewCount, lastSuccessRate);
        float timeDecayFactor = MathF.Exp(-daysSinceReview * FORGETTING_CURVE_COEFFICIENT / memoryStrengthFactor);

        // ПОМІРНІ максимальні штрафи
        float maxPenalty = baseRate < 2.5f ? 0.6f :      // було 1.2f
                          baseRate > 4.0f ? 1.5f :       // було 2.5f
                          1.0f;                          // було 1.8f

        float penalty = maxPenalty * (1 - timeDecayFactor);

        return Math.Min(penalty, maxPenalty);
    }

    private static float CalculateMemoryStrength(int reviewCount, float lastSuccessRate)
    {
        float baseStrength = 1.0f + (reviewCount * 0.2f);  // повернуто до початкового
        float successModifier = 0.5f + (lastSuccessRate * 0.5f);

        return baseStrength * successModifier;
    }
}
