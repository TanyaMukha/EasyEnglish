namespace EasyEnglish.App.Services;

/// <summary>
/// Сервіс для управління процесом навчання слів з підтримкою сесій
/// </summary>
public static class WordLearningService
{
    public static int GetTestPriority((CardType cardType, CardDirection direction) type)
    {
        return type switch
        {
            (CardType.Review, CardDirection.WordToTranslation) => -1,
            (CardType.QuickAnswer, CardDirection.WordToTranslation) => 0,
            (CardType.QuickAnswer, CardDirection.TranslationToWord) => 1,
            (CardType.SingleChoice, CardDirection.WordToTranslation) => 2,
            (CardType.SingleChoice, CardDirection.TranslationToWord) => 3,
            (CardType.KnowOrNot, CardDirection.WordToTranslation) => 4,
            (CardType.KnowOrNot, CardDirection.TranslationToWord) => 5,
            (CardType.ManualInput, CardDirection.TranslationToWord) => 6,
            _ => throw new ArgumentException("Unknown card configuration")
        };
    }
    
    /// <summary>
    /// Маппінг з CardType в назву сторінки
    /// </summary>
    public static string GetPageRoute(CardType cardType, CardDirection direction)
    {
        return (cardType, direction) switch
        {
            (CardType.SingleChoice, CardDirection.WordToTranslation) => "word-to-translation-single-choice",
            (CardType.SingleChoice, CardDirection.TranslationToWord) => "translation-to-word-single-choice",
            (CardType.KnowOrNot, CardDirection.WordToTranslation) => "word-to-translation-know-or-not",
            (CardType.KnowOrNot, CardDirection.TranslationToWord) => "translation-to-word-know-or-not",
            (CardType.ManualInput, CardDirection.TranslationToWord) => "translation-to-word-manual-input",
            _ => throw new ArgumentException("Unknown card configuration")
        };
    }

    /// <summary>
    /// Зворотній маппінг з назви сторінки в CardType та CardDirection
    /// </summary>
    public static (CardType Type, CardDirection Direction) ParsePageRoute(string route)
    {
        return route switch
        {
            "word-to-translation-single-choice" => (CardType.SingleChoice, CardDirection.WordToTranslation),
            "translation-to-word-single-choice" => (CardType.SingleChoice, CardDirection.TranslationToWord),
            "word-to-translation-know-or-not" => (CardType.KnowOrNot, CardDirection.WordToTranslation),
            "translation-to-word-know-or-not" => (CardType.KnowOrNot, CardDirection.TranslationToWord),
            "translation-to-word-manual-input" => (CardType.ManualInput, CardDirection.TranslationToWord),
            _ => throw new ArgumentException($"Unknown route: {route}")
        };
    }

    public static string GetColorClass(CardType cardType, CardDirection direction)
    {
        return (cardType, direction) switch
        {
            (CardType.SingleChoice, CardDirection.WordToTranslation) => "pastel-green",
            (CardType.SingleChoice, CardDirection.TranslationToWord) => "pastel-purple",
            (CardType.KnowOrNot, CardDirection.WordToTranslation) => "pastel-pink",
            (CardType.KnowOrNot, CardDirection.TranslationToWord) => "pastel-yellow",
            (CardType.ManualInput, CardDirection.TranslationToWord) => "pastel-mint",
            _ => "pastel-blue"
        };
    }

    public static string GetCardIcon(CardType cardType, CardDirection direction)
    {
        return (cardType, direction) switch
        {
            (CardType.SingleChoice, CardDirection.WordToTranslation) => "bi-translate",
            (CardType.SingleChoice, CardDirection.TranslationToWord) => "bi-list-check",
            (CardType.KnowOrNot, CardDirection.WordToTranslation) => "bi-spellcheck",
            (CardType.KnowOrNot, CardDirection.TranslationToWord) => "bi-check-circle",
            (CardType.ManualInput, CardDirection.TranslationToWord) => "bi-keyboard",
            _ => "bi-book"
        };
    }
}