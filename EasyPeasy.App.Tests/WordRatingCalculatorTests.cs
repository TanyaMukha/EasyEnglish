using EasyPeasy.App.Models;
using EasyPeasy.App.Services;

namespace EasyPeasy.App.Tests;

public class WordRatingCalculatorTests
{
    private static WordTestModel NewWord(float rate = 3f, DateTime? lastReviewDate = null, int reviewCount = 0,
        int lastTotalAttempts = 0, int lastIncorrectAttempts = 0) => new()
    {
        Rate = rate,
        LastReviewDate = lastReviewDate,
        ReviewCount = reviewCount,
        LastTotalAttempts = lastTotalAttempts,
        LastIncorrectAttempts = lastIncorrectAttempts,
    };

    [Theory]
    [InlineData(CardType.SingleChoice, new[] { CardDirection.WordToTranslation, CardDirection.TranslationToWord })]
    [InlineData(CardType.KnowOrNot, new[] { CardDirection.WordToTranslation, CardDirection.TranslationToWord })]
    [InlineData(CardType.ManualInput, new[] { CardDirection.TranslationToWord })]
    [InlineData(CardType.Pronunciation, new[] { CardDirection.TranslationToWord })]
    public void GetAvailableDirections_ReturnsExpectedDirections(CardType type, CardDirection[] expected)
    {
        var result = WordRatingCalculator.GetAvailableDirections(type);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(CardType.MultipleChoice)]
    [InlineData(CardType.Matching)]
    public void GetAvailableDirections_UnmappedCardType_ReturnsEmpty(CardType type)
    {
        var result = WordRatingCalculator.GetAvailableDirections(type);

        Assert.Empty(result);
    }

    [Fact]
    public void CalculateCurrentRate_NeverReviewed_ReturnsStoredRateUnchanged()
    {
        var word = NewWord(rate: 3.7f, lastReviewDate: null);

        var result = WordRatingCalculator.CalculateCurrentRate(word);

        Assert.Equal(3.7f, result);
    }

    [Fact]
    public void CalculateCurrentRate_ReviewedToday_ReturnsStoredRateUnchanged()
    {
        var now = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var word = NewWord(rate: 3.7f, lastReviewDate: now.AddHours(-2));

        var result = WordRatingCalculator.CalculateCurrentRate(word, now);

        Assert.Equal(3.7f, result);
    }

    [Fact]
    public void CalculateCurrentRate_UnreviewedForDays_IncreasesRate()
    {
        var now = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var word = NewWord(rate: 3.0f, lastReviewDate: now.AddDays(-10));

        var result = WordRatingCalculator.CalculateCurrentRate(word, now);

        Assert.True(result > 3.0f);
        Assert.True(result <= 5.0f);
    }

    [Fact]
    public void CalculateCurrentRate_MatchesDocumentedForgettingCurveFormula()
    {
        // baseRate=3.0 -> maxPenalty tier = 1.0 (not <2.5, not >4.0); reviewCount=0,
        // lastTotalAttempts=0 -> lastSuccessRate defaults to 0.5.
        var now = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var word = NewWord(rate: 3.0f, lastReviewDate: now.AddDays(-10), reviewCount: 0,
            lastTotalAttempts: 0, lastIncorrectAttempts: 0);

        var result = WordRatingCalculator.CalculateCurrentRate(word, now);

        var memoryStrength = (1.0 + 0 * 0.2) * (0.5 + 0.5 * 0.5); // 0.75
        var timeDecay = MathF.Exp((float)(-10 * 0.05 / memoryStrength));
        var expected = 3.0f + 1.0f * (1 - timeDecay);

        Assert.Equal(expected, result, precision: 4);
    }

    [Fact]
    public void CalculateCurrentRate_HigherReviewCountAndSuccessRate_DecaysSlower()
    {
        var now = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var weak = NewWord(rate: 3.0f, lastReviewDate: now.AddDays(-10), reviewCount: 0,
            lastTotalAttempts: 4, lastIncorrectAttempts: 4);
        var strong = NewWord(rate: 3.0f, lastReviewDate: now.AddDays(-10), reviewCount: 10,
            lastTotalAttempts: 4, lastIncorrectAttempts: 0);

        var weakResult = WordRatingCalculator.CalculateCurrentRate(weak, now);
        var strongResult = WordRatingCalculator.CalculateCurrentRate(strong, now);

        Assert.True(strongResult < weakResult);
    }

    [Fact]
    public void CalculateCurrentRate_ClampsToMaxRate()
    {
        var now = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var word = NewWord(rate: 4.9f, lastReviewDate: now.AddYears(-5));

        var result = WordRatingCalculator.CalculateCurrentRate(word, now);

        Assert.Equal(5.0f, result);
    }

    [Fact]
    public void UpdateWordAfterSession_NullTests_ReturnsWordUnchanged()
    {
        var word = new WordTestModel { Rate = 3f, Tests = null! };

        var result = WordRatingCalculator.UpdateWordAfterSession(word);

        Assert.Same(word, result);
        Assert.Equal(3f, result.Rate);
        Assert.Null(result.LastReviewDate);
    }

    [Fact]
    public void UpdateWordAfterSession_ZeroAttempts_IsNoOp()
    {
        var word = NewWord(rate: 3f);

        var result = WordRatingCalculator.UpdateWordAfterSession(word);

        Assert.Equal(3f, result.Rate);
        Assert.Null(result.LastReviewDate);
        Assert.Equal(0, result.ReviewCount);
    }

    [Fact]
    public void UpdateWordAfterSession_PerfectManualInputTranslationToWord_DecreasesRateByWeightedAmount()
    {
        var word = NewWord(rate: 3.0f);
        word.Tests[CardDirection.TranslationToWord][CardType.ManualInput] =
            new TestResult { TotalAttempts = 10, CorrectAnswers = 10 };

        var result = WordRatingCalculator.UpdateWordAfterSession(word);

        // baseChange = -0.585 (>=95% correct); impact(ManualInput) = 1.5;
        // weight = cardWeight(1.0) * directionWeight(1.0) = 1.0, so the weighted average keeps the
        // impact-scaled change as is; attemptModifier = 0.7 + min(10/12, 0.35) = 1.05
        // finalRateChange = -0.585 * 1.5 * 1.05 = -0.921375
        Assert.Equal(3.0f - 0.921375f, result.Rate, precision: 4);
        Assert.Equal(10, result.LastTotalAttempts);
        Assert.Equal(0, result.LastIncorrectAttempts);
        Assert.Equal(1, result.ReviewCount);
        Assert.NotNull(result.LastReviewDate);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public void UpdateWordAfterSession_TypedAnswersMoveTheRateMoreThanRecognition()
    {
        // The point of the card-type impact factors: the same success rate over the same number of
        // attempts must count for more when the learner produced the word from memory than when
        // they picked it out of a list or just claimed to know it.
        static float DropFor(CardType type)
        {
            var word = NewWord(rate: 4.0f);
            word.Tests[CardDirection.TranslationToWord][type] =
                new TestResult { TotalAttempts = 6, CorrectAnswers = 6 };

            return 4.0f - WordRatingCalculator.UpdateWordAfterSession(word).Rate;
        }

        var typed       = DropFor(CardType.ManualInput);
        var spoken      = DropFor(CardType.Pronunciation);
        var recognised  = DropFor(CardType.SingleChoice);
        var selfClaimed = DropFor(CardType.KnowOrNot);

        Assert.Equal(typed, spoken, precision: 4);
        Assert.True(typed > recognised);
        Assert.True(recognised > selfClaimed);
    }

    [Fact]
    public void UpdateWordAfterSession_MixedSession_LandsBetweenTheSingleTypeExtremes()
    {
        // A mixed session is a weighted average, so it cannot move the rate further than a session
        // made only of the strongest card type in it.
        static float DropFor(params (CardType Type, int Total, int Correct)[] results)
        {
            var word = NewWord(rate: 4.0f);
            foreach (var (type, total, correct) in results)
                word.Tests[CardDirection.TranslationToWord][type] =
                    new TestResult { TotalAttempts = total, CorrectAnswers = correct };

            return 4.0f - WordRatingCalculator.UpdateWordAfterSession(word).Rate;
        }

        var typedOnly = DropFor((CardType.ManualInput, 6, 6));
        var mixed     = DropFor((CardType.ManualInput, 6, 6), (CardType.KnowOrNot, 6, 6));

        Assert.True(mixed < typedOnly);
        Assert.True(mixed > 0);
    }

    [Fact]
    public void UpdateWordAfterSession_AllWrongAnswers_IncreasesRate()
    {
        var word = NewWord(rate: 3.0f);
        word.Tests[CardDirection.TranslationToWord][CardType.ManualInput] =
            new TestResult { TotalAttempts = 4, CorrectAnswers = 0 };

        var result = WordRatingCalculator.UpdateWordAfterSession(word);

        Assert.True(result.Rate > 3.0f);
        Assert.Equal(4, result.LastTotalAttempts);
        Assert.Equal(4, result.LastIncorrectAttempts);
    }

    [Fact]
    public void UpdateWordAfterSession_MultipleCombinations_SumsAttemptsAcrossThem()
    {
        var word = NewWord(rate: 3.0f);
        word.Tests[CardDirection.TranslationToWord][CardType.ManualInput] =
            new TestResult { TotalAttempts = 10, CorrectAnswers = 10 };
        word.Tests[CardDirection.WordToTranslation][CardType.KnowOrNot] =
            new TestResult { TotalAttempts = 5, CorrectAnswers = 0 };

        var result = WordRatingCalculator.UpdateWordAfterSession(word);

        Assert.Equal(15, result.LastTotalAttempts);
        Assert.Equal(5, result.LastIncorrectAttempts);
        // Only one review pass is recorded even though two combinations contributed.
        Assert.Equal(1, result.ReviewCount);
    }

    [Fact]
    public void UpdateWordAfterSession_ClampsToMinRate()
    {
        var word = NewWord(rate: 0.1f);
        word.Tests[CardDirection.TranslationToWord][CardType.ManualInput] =
            new TestResult { TotalAttempts = 1, CorrectAnswers = 1 };

        var result = WordRatingCalculator.UpdateWordAfterSession(word);

        Assert.Equal(0.0f, result.Rate);
    }

    [Fact]
    public void UpdateWordAfterSession_ClampsToMaxRate()
    {
        var word = NewWord(rate: 4.9f);
        word.Tests[CardDirection.TranslationToWord][CardType.ManualInput] =
            new TestResult { TotalAttempts = 100, CorrectAnswers = 0 };

        var result = WordRatingCalculator.UpdateWordAfterSession(word);

        Assert.Equal(5.0f, result.Rate);
    }

    [Fact]
    public void UpdateWordRate_SkipsItemsWithNullTests()
    {
        var withTests = NewWord(rate: 3.0f);
        withTests.Tests[CardDirection.TranslationToWord][CardType.ManualInput] =
            new TestResult { TotalAttempts = 1, CorrectAnswers = 1 };
        var withoutTests = new WordTestModel { Rate = 3f, Tests = null! };

        var result = WordRatingCalculator.UpdateWordRate([withTests, withoutTests]);

        Assert.Single(result);
        Assert.Same(withTests, result[0]);
    }

    [Fact]
    public void UpdateWordRate_IncludesUnchangedZeroAttemptItems()
    {
        var untouched = NewWord(rate: 3.0f);

        var result = WordRatingCalculator.UpdateWordRate([untouched]);

        Assert.Single(result);
        Assert.Equal(3.0f, result[0].Rate);
    }

    [Fact]
    public void UpdateWordRate_EmptyList_ReturnsEmptyList()
    {
        var result = WordRatingCalculator.UpdateWordRate(new List<WordTestModel>());

        Assert.Empty(result);
    }

    [Fact]
    public void UpdateWordRate_NullItems_Throws()
    {
        // Was previously swallowed into an empty list (Known Issues #1 in
        // EasyPeasy.App/Services/README.md, since fixed) -- a caller passing null is a real bug
        // and should surface as one instead of silently looking like "nothing to update."
        Assert.Throws<NullReferenceException>(() => WordRatingCalculator.UpdateWordRate<WordTestModel>(null!));
    }

    [Fact]
    public void UpdateWordAfterSession_ProcessesTrackableCardTypeNormally()
    {
        // TrackableCardTypes (Known Issue #2, fixed) used to exist alongside two synthetic,
        // sort-priority-only CardType values (Review/QuickAnswer, since removed along with their
        // sole consumer, WordLearningService) that TestDetailModel's indexer never supported.
        // Kept as a baseline confirming a normal, real CardType still processes correctly through
        // the allowlist.
        var word = NewWord(rate: 3.0f);
        word.Tests[CardDirection.TranslationToWord][CardType.ManualInput] =
            new TestResult { TotalAttempts = 1, CorrectAnswers = 1 };

        var result = WordRatingCalculator.UpdateWordAfterSession(word);

        Assert.Equal(1, result.LastTotalAttempts);
    }
}
