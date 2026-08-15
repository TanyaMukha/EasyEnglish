using EasyEnglish.App.Models;

namespace EasyEnglish.App.Services;

/// <summary>Kind of drilling card used to review a word.</summary>
public enum CardType
{
    /// <summary>Self-assessed "I know it" / "I don't know it" recall check.</summary>
    KnowOrNot = 0,
    /// <summary>Free-text typed answer.</summary>
    ManualInput = 1,
    /// <summary>Pick one correct option out of several.</summary>
    SingleChoice = 2,
    /// <summary>Pick several correct options out of several.</summary>
    MultipleChoice = 3,
    /// <summary>Match pairs between two columns.</summary>
    Matching = 4,
    /// <summary>Spoken-pronunciation check via microphone.</summary>
    Pronunciation = 5
}

/// <summary>Which side of the word/translation pair is shown as the prompt.</summary>
public enum CardDirection
{
    /// <summary>English word shown → learner answers with the translation.</summary>
    WordToTranslation = 0,
    /// <summary>Translation shown → learner answers with the English word.</summary>
    TranslationToWord = 1,
}

/// <summary>
/// Turns a drilling session's per-card results into an updated <see cref="ITestSessionItem.Rate"/>
/// (difficulty on the <c>[0, 5]</c> scale — see <c>EasyEnglish.Core.Extensions.RateExtensions</c> for
/// how that scale buckets into <c>DifficultyLevel</c>). Two effects are combined: <see cref="UpdateWordAfterSession{T}"/>
/// applies a rate change immediately after a session based on how well the learner did, weighted by
/// card type/direction; <see cref="CalculateCurrentRate{T}"/> separately estimates a "decayed" rate
/// that increases over time since <see cref="ITestSessionItem.LastReviewDate"/> (a simple forgetting-curve
/// model), for ranking which words are most in need of review right now without writing anything back.
/// </summary>
public static class WordRatingCalculator
{
    private const float MIN_RATE = 0.0f;
    private const float MAX_RATE = 5.0f;

    // Tuned by hand for a smooth, not-too-jumpy difficulty response; scaled x1.3 from the previous
    // values (themselves x1.5 from the originals) because a whole session still moved the rate too
    // little to matter. See EasyEnglish.App/Services/README.md for the tuning history in git blame.
    private const float PERFECT_ANSWER_DECREASE = 0.585f; // 95-100% correct
    private const float GOOD_ANSWER_DECREASE = 0.39f;     // 75-94% correct
    private const float AVERAGE_ANSWER_DECREASE = 0.195f; // 65-74% correct
    private const float POOR_ANSWER_INCREASE = 0.29f;     // 50-64% correct
    private const float BAD_ANSWER_INCREASE = 0.49f;      // 25-49% correct
    private const float FAILED_ANSWER_INCREASE = 0.78f;   // 0-24% correct

    private const float EASY_WORD_MULTIPLIER = 0.7f;
    private const float HARD_WORD_MULTIPLIER = 1.5f;

    private const float FORGETTING_CURVE_COEFFICIENT = 0.05f;

    /// <summary>
    /// How much each card type counts toward the session's overall rate change — free-text/spoken
    /// recall counts fullest, passive self-assessment counts least.
    /// These are averaging weights only: they decide whose opinion dominates when a session mixes
    /// card types, and cancel out entirely when a session uses just one type. How far the rate
    /// actually moves is <see cref="_cardTypeImpact"/>.
    /// </summary>
    private static readonly Dictionary<CardType, float> _cardTypeWeights = new()
    {
        { CardType.SingleChoice, 0.5f },
        { CardType.KnowOrNot, 0.3f },
        { CardType.ManualInput, 1.0f },
        { CardType.Pronunciation, 1.0f }
    };

    /// <summary>
    /// How strongly each card type moves the rate. Unlike <see cref="_cardTypeWeights"/> this is a
    /// magnitude, not a share: producing the word from memory (typing it, saying it) is real
    /// evidence about whether the learner knows it, so it shifts the rate far more than recognising
    /// the right option out of four, and much more than self-reported "I knew this one".
    /// </summary>
    private static readonly Dictionary<CardType, float> _cardTypeImpact = new()
    {
        { CardType.KnowOrNot, 0.7f },
        { CardType.SingleChoice, 0.9f },
        { CardType.MultipleChoice, 1.0f },
        { CardType.Matching, 1.0f },
        { CardType.ManualInput, 1.5f },
        { CardType.Pronunciation, 1.5f }
    };

    /// <summary>How much each direction counts toward the session's overall rate change — recalling the word from its translation counts more than the reverse.</summary>
    private static readonly Dictionary<CardDirection, float> _directionWeights = new()
    {
        { CardDirection.WordToTranslation, 0.6f },
        { CardDirection.TranslationToWord, 1.0f },
    };

    /// <summary>
    /// The <see cref="CardType"/> values that <see cref="TestDetailModel"/> actually records a
    /// <see cref="TestResult"/> for — currently all of them, but kept as an explicit allowlist
    /// (rather than <c>Enum.GetValues&lt;CardType&gt;()</c>) so that if a future synthetic,
    /// sort-priority-only value gets added to <see cref="CardType"/> without a matching
    /// <see cref="TestDetailModel"/> case (as happened before with the now-removed
    /// <c>Review</c>/<c>QuickAnswer</c> values), <see cref="UpdateWordAfterSession{T}"/> keeps
    /// skipping it by construction instead of needing a defensive <c>try</c>/<c>catch</c> again.
    /// </summary>
    private static readonly CardType[] TrackableCardTypes =
    [
        CardType.KnowOrNot,
        CardType.ManualInput,
        CardType.SingleChoice,
        CardType.MultipleChoice,
        CardType.Matching,
        CardType.Pronunciation,
    ];

    /// <summary>Which <see cref="CardDirection"/>s make sense for a given <see cref="CardType"/> — used to build a drilling session's card list.</summary>
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
            CardType.Pronunciation => new List<CardDirection>
            {
                CardDirection.TranslationToWord
            },
            _ => new List<CardDirection>()
        };
    }

    /// <summary>
    /// Base rate delta for a single card's success rate, before card-type impact and weighting are
    /// applied. Negative = gets easier (rate decreases); positive = gets harder (rate increases).
    /// </summary>
    private static float CalculateRateChange(float successRate)
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
    /// Estimates today's effective difficulty for <paramref name="word"/> by aging its stored
    /// <see cref="ITestSessionItem.Rate"/> toward "harder" the longer it's gone unreviewed (a simple
    /// forgetting-curve model) — read-only, does not persist anything. Returns the stored rate as-is
    /// if the word has never been reviewed, or was reviewed today.
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

    /// <summary>
    /// Batch version of <see cref="UpdateWordAfterSession{T}"/> — updates every item in
    /// <paramref name="items"/> that has session test data (items with none are skipped, not
    /// included in the result). Any exception from processing a single item propagates rather than
    /// being swallowed — a genuine failure should surface loudly instead of silently dropping every
    /// update in the batch (see EasyEnglish.App/Services/README.md Known Issues #1, now fixed).
    /// </summary>
    public static List<T> UpdateWordRate<T>(List<T> items)
        where T : ITestSessionItem
    {
        var wordsToUpdate = new List<T>();

        foreach (var word in items)
        {
            if (word.Tests == null)
                continue;

            var updatedWord = UpdateWordAfterSession(word);

            wordsToUpdate.Add(updatedWord);
        }

        return wordsToUpdate;
    }

    /// <summary>
    /// Folds one item's session test results (every <see cref="CardDirection"/> × <see cref="CardType"/>
    /// combination attempted) into a single weighted rate change, and applies it — mutating and
    /// returning <paramref name="word"/> in place (also bumps <see cref="ITestSessionItem.LastReviewDate"/>,
    /// <see cref="ITestSessionItem.ReviewCount"/>, and the attempt counters). A no-op (returns
    /// <paramref name="word"/> unchanged) if <see cref="ITestSessionItem.Tests"/> is <c>null</c> or
    /// has zero total attempts across every combination.
    /// </summary>
    /// <param name="word">The session item with recorded test results.</param>
    /// <returns>The same instance, updated and ready to persist.</returns>
    public static T UpdateWordAfterSession<T>(T word)
        where T : ITestSessionItem
    {
        if (word.Tests == null)
            return word;

        // Aggregate results across every direction × card type combination attempted this session.
        int totalAttempts = 0;
        int totalCorrect = 0;
        float weightedRateChange = 0f;
        float totalWeight = 0f;

        foreach (CardDirection direction in Enum.GetValues<CardDirection>())
        {
            var testDetail = word.Tests[direction];

            if (testDetail == null)
                continue;

            foreach (CardType cardType in TrackableCardTypes)
            {
                var testResult = testDetail[cardType];

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

                // Impact scales the change itself, weight only decides this card's share of the
                // average — so a session made entirely of typed answers still moves the rate more
                // than one made entirely of multiple choice.
                float impact = _cardTypeImpact.GetValueOrDefault(cardType, 1.0f);

                weightedRateChange += CalculateRateChange(successRate) * impact * weight;
                totalWeight += weight;
            }
        }

        // Only touch the rate/stats if at least one combination had a real attempt this session.
        if (totalAttempts > 0 && totalWeight > 0)
        {
            float averageRateChange = weightedRateChange / totalWeight;

            // Moderate scaling by attempt count, capped so a single big session can't swing the
            // rate too far at once (tuning history: see git blame for prior constants).
            // Raised from 0.6 + max 0.25: the old ceiling of 0.85 meant even a long, flawless
            // session was damped below its own base change, which made progress feel invisible.
            float attemptModifier = 0.7f + Math.Min(totalAttempts / 12.0f, 0.35f);
            float finalRateChange = averageRateChange * attemptModifier;

            word.Rate = Math.Clamp(word.Rate + finalRateChange, MIN_RATE, MAX_RATE);

            // Update review bookkeeping.
            word.LastReviewDate = DateTime.UtcNow;
            word.LastTotalAttempts = totalAttempts;
            word.LastIncorrectAttempts = totalAttempts - totalCorrect;
            word.ReviewCount++;
            word.UpdatedAt = DateTime.UtcNow;
        }

        return word;
    }

    /// <summary>
    /// Estimates how much <paramref name="baseRate"/> should be discounted for the days elapsed
    /// since the last review, modeling an exponential forgetting curve
    /// (<c>maxPenalty * (1 - e^(-days * FORGETTING_CURVE_COEFFICIENT / memoryStrength))</c>): the
    /// penalty grows with <paramref name="daysSinceReview"/> but saturates at <c>maxPenalty</c>,
    /// and decays more slowly the stronger <see cref="CalculateMemoryStrength"/> reports the item's
    /// memory strength to be. <paramref name="maxPenalty"/> is tiered by <paramref name="baseRate"/>
    /// so already-weak items (rate &lt; 2.5) can't be penalized as harshly as strong ones (rate &gt; 4.0),
    /// since they have less room to fall before hitting <see cref="MIN_RATE"/>.
    /// </summary>
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

        float maxPenalty = baseRate < 2.5f ? 0.6f :
                          baseRate > 4.0f ? 1.5f :
                          1.0f;

        float penalty = maxPenalty * (1 - timeDecayFactor);

        return Math.Min(penalty, maxPenalty);
    }

    /// <summary>
    /// Combines review frequency and past success rate into a single "how well-learned is this
    /// item" factor, used by <see cref="CalculateForgettingPenalty"/> to slow down the forgetting
    /// curve for items reviewed often and answered correctly: each review adds
    /// <c>0.2</c> to a base strength of <c>1.0</c>, then scaled by a <c>[0.5, 1.0]</c> factor
    /// derived from <paramref name="lastSuccessRate"/>.
    /// </summary>
    private static float CalculateMemoryStrength(int reviewCount, float lastSuccessRate)
    {
        float baseStrength = 1.0f + (reviewCount * 0.2f);
        float successModifier = 0.5f + (lastSuccessRate * 0.5f);

        return baseStrength * successModifier;
    }
}
