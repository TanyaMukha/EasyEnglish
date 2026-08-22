using EasyPeasy.Core.Enums;
using EasyPeasy.Core.Models;

namespace EasyPeasy.Core.Tests;

/// <summary>
/// Tests for <see cref="UnitModel.ClearKeyFields"/>, <see cref="UnitModel.ClearLearningProgress"/>,
/// and <see cref="UnitModel.RemoveExamples"/> — covers the fix for the bug where these methods only
/// touched <c>Words</c> and silently left <c>IrregularForms</c>/<c>StudyCards</c>/<c>TestCards</c>
/// untouched (see the library's README, Known Issue #1).
/// </summary>
public class UnitModelTests
{
    private static readonly DateTime OldTimestamp = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ReviewDate = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    private static UnitModel BuildUnit() => new()
    {
        Id = 10,
        CourseId = 5,
        CreatedAt = OldTimestamp,
        UpdatedAt = OldTimestamp,
        Words =
        [
            new WordModel { Id = 1, UnitId = 10, CreatedAt = OldTimestamp, UpdatedAt = OldTimestamp, Rate = 4.5f, LastReviewDate = ReviewDate, ReviewCount = 3 },
        ],
        IrregularForms =
        [
            new IrregularFormModel { Id = 2, UnitId = 10, CreatedAt = OldTimestamp, UpdatedAt = OldTimestamp, Rate = 4.5f, LastReviewDate = ReviewDate, ReviewCount = 3 },
        ],
        StudyCards =
        [
            new StudyCardModel { Id = 3, UnitId = 10, CreatedAt = OldTimestamp, UpdatedAt = OldTimestamp, Rate = 4.5f, LastReviewDate = ReviewDate, ReviewCount = 3 },
        ],
        TestCards =
        [
            new TestCardModel { Id = 4, UnitId = 10, Kind = TestCardKind.ShortAnswer, CreatedAt = OldTimestamp, UpdatedAt = OldTimestamp, Rate = 4.5f, LastReviewDate = ReviewDate, ReviewCount = 3 },
        ],
    };

    [Fact]
    public void ClearKeyFields_ResetsUnitIdentity()
    {
        var unit = BuildUnit();

        unit.ClearKeyFields();

        Assert.Equal(0, unit.Id);
        Assert.Equal(0, unit.CourseId);
        Assert.True(unit.CreatedAt > OldTimestamp);
        Assert.Null(unit.UpdatedAt);
    }

    [Fact]
    public void ClearKeyFields_ResetsWordIdentity()
    {
        var unit = BuildUnit();

        unit.ClearKeyFields();

        Assert.Equal(0, unit.Words![0].Id);
        Assert.Equal(0, unit.Words![0].UnitId);
        Assert.True(unit.Words![0].CreatedAt > OldTimestamp);
        Assert.Null(unit.Words![0].UpdatedAt);
    }

    [Fact]
    public void ClearKeyFields_ResetsIrregularFormIdentity()
    {
        var unit = BuildUnit();

        unit.ClearKeyFields();

        Assert.Equal(0, unit.IrregularForms![0].Id);
        Assert.Equal(0, unit.IrregularForms![0].UnitId);
        Assert.True(unit.IrregularForms![0].CreatedAt > OldTimestamp);
        Assert.Null(unit.IrregularForms![0].UpdatedAt);
    }

    [Fact]
    public void ClearKeyFields_ResetsStudyCardIdentity()
    {
        var unit = BuildUnit();

        unit.ClearKeyFields();

        Assert.Equal(0, unit.StudyCards![0].Id);
        Assert.Equal(0, unit.StudyCards![0].UnitId);
        Assert.True(unit.StudyCards![0].CreatedAt > OldTimestamp);
        Assert.Null(unit.StudyCards![0].UpdatedAt);
    }

    [Fact]
    public void ClearKeyFields_ResetsTestCardIdentity()
    {
        var unit = BuildUnit();

        unit.ClearKeyFields();

        Assert.Equal(0, unit.TestCards![0].Id);
        Assert.Equal(0, unit.TestCards![0].UnitId);
        Assert.True(unit.TestCards![0].CreatedAt > OldTimestamp);
        Assert.Null(unit.TestCards![0].UpdatedAt);
    }

    [Fact]
    public void ClearKeyFields_ReturnsSameInstance_ForChaining()
    {
        var unit = BuildUnit();

        var result = unit.ClearKeyFields();

        Assert.Same(unit, result);
    }

    [Fact]
    public void ClearKeyFields_NullChildCollections_DoesNotThrow()
    {
        var unit = new UnitModel { Id = 1, CourseId = 1 };

        var exception = Record.Exception(() => unit.ClearKeyFields());

        Assert.Null(exception);
    }

    [Fact]
    public void ClearLearningProgress_ResetsUnitReviewState()
    {
        var unit = BuildUnit();
        unit.LastReviewDate = ReviewDate;
        unit.ReviewCount = 5;

        unit.ClearLearningProgress();

        Assert.Null(unit.LastReviewDate);
        Assert.Equal(0, unit.ReviewCount);
    }

    [Fact]
    public void ClearLearningProgress_ResetsWordProgress()
    {
        var unit = BuildUnit();

        unit.ClearLearningProgress();

        Assert.Equal(3.0f, unit.Words![0].Rate);
        Assert.Null(unit.Words![0].LastReviewDate);
        Assert.Equal(0, unit.Words![0].ReviewCount);
    }

    [Fact]
    public void ClearLearningProgress_ResetsIrregularFormProgress()
    {
        var unit = BuildUnit();

        unit.ClearLearningProgress();

        Assert.Equal(3.0f, unit.IrregularForms![0].Rate);
        Assert.Null(unit.IrregularForms![0].LastReviewDate);
        Assert.Equal(0, unit.IrregularForms![0].ReviewCount);
    }

    [Fact]
    public void ClearLearningProgress_ResetsStudyCardProgress()
    {
        var unit = BuildUnit();

        unit.ClearLearningProgress();

        Assert.Equal(3.0f, unit.StudyCards![0].Rate);
        Assert.Null(unit.StudyCards![0].LastReviewDate);
        Assert.Equal(0, unit.StudyCards![0].ReviewCount);
    }

    [Fact]
    public void ClearLearningProgress_ResetsTestCardProgress()
    {
        var unit = BuildUnit();

        unit.ClearLearningProgress();

        Assert.Equal(3.0f, unit.TestCards![0].Rate);
        Assert.Null(unit.TestCards![0].LastReviewDate);
        Assert.Equal(0, unit.TestCards![0].ReviewCount);
    }

    [Fact]
    public void ClearLearningProgress_NullChildCollections_DoesNotThrow()
    {
        var unit = new UnitModel();

        var exception = Record.Exception(() => unit.ClearLearningProgress());

        Assert.Null(exception);
    }

    [Fact]
    public void RemoveExamples_ClearsExamplesOnEveryWord()
    {
        var unit = BuildUnit();
        unit.Words![0].Examples = [new ExampleModel { Id = 1, WordId = 1 }];

        unit.RemoveExamples();

        Assert.Empty(unit.Words![0].Examples!);
    }

    [Fact]
    public void RemoveExamples_NullWords_DoesNotThrow()
    {
        var unit = new UnitModel();

        var exception = Record.Exception(() => unit.RemoveExamples());

        Assert.Null(exception);
    }
}
