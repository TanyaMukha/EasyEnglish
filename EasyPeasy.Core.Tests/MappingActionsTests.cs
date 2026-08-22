using AutoMapper;
using EasyPeasy.Core.Enums;
using EasyPeasy.Core.Mapping;
using EasyPeasy.Core.Models;
using EasyPeasy.Core.Tests.Fixtures;

namespace EasyPeasy.Core.Tests;

/// <summary>
/// Tests for the <c>UnitModel → UnitModel</c> self-map and its <see cref="UnitMappingOptions"/>-driven
/// <c>IMappingAction</c>s (<see cref="MappingActions"/>) — the logic behind cloning a unit while
/// selectively resetting identity/learning-progress per child collection.
/// </summary>
public class MappingActionsTests
{
    private static readonly IMapper Mapper = MapperFactory.Instance;
    private static readonly DateTime OldTimestamp = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static UnitModel BuildUnit()
    {
        var reviewDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        return new UnitModel
        {
            Id = 10,
            CourseId = 5,
            RecordGuid = Guid.NewGuid(),
            CreatedAt = OldTimestamp,
            UpdatedAt = OldTimestamp,
            Words =
            [
                new WordModel
                {
                    Id = 1,
                    UnitId = 10,
                    Rate = 4.5f,
                    LastReviewDate = reviewDate,
                    ReviewCount = 3,
                    CreatedAt = OldTimestamp,
                    UpdatedAt = OldTimestamp,
                    Examples = [new ExampleModel { Id = 100, WordId = 1 }],
                },
            ],
            IrregularForms =
            [
                new IrregularFormModel
                {
                    Id = 2,
                    UnitId = 10,
                    Rate = 4.5f,
                    LastReviewDate = reviewDate,
                    ReviewCount = 3,
                    CreatedAt = OldTimestamp,
                    UpdatedAt = OldTimestamp,
                },
            ],
            StudyCards =
            [
                new StudyCardModel
                {
                    Id = 3,
                    UnitId = 10,
                    Rate = 4.5f,
                    LastReviewDate = reviewDate,
                    ReviewCount = 3,
                    CreatedAt = OldTimestamp,
                    UpdatedAt = OldTimestamp,
                },
            ],
            TestCards =
            [
                new TestCardModel
                {
                    Id = 4,
                    UnitId = 10,
                    Kind = TestCardKind.ShortAnswer,
                    Rate = 4.5f,
                    LastReviewDate = reviewDate,
                    ReviewCount = 3,
                    CreatedAt = OldTimestamp,
                    UpdatedAt = OldTimestamp,
                },
            ],
        };
    }

    [Fact]
    public void NoOptionsProvided_IsNoOp()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source);

        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.CourseId, result.CourseId);
        Assert.Equal(source.RecordGuid, result.RecordGuid);
        Assert.Equal(source.CreatedAt, result.CreatedAt);
        Assert.Equal(source.Words![0].Id, result.Words![0].Id);
        Assert.Equal(source.IrregularForms![0].Id, result.IrregularForms![0].Id);
        Assert.Equal(source.StudyCards![0].Id, result.StudyCards![0].Id);
        Assert.Equal(source.TestCards![0].Id, result.TestCards![0].Id);
    }

    [Fact]
    public void EmptyOptions_AllFlagsFalse_IsPracticalNoOp()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions());

        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.RecordGuid, result.RecordGuid);
        Assert.Equal(source.Words![0].Id, result.Words![0].Id);
    }

    [Fact]
    public void UnitResetId_ResetsUnitIdentityButNotChildren()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions { ResetId = true });

        Assert.Equal(0, result.Id);
        Assert.Equal(0, result.CourseId);
        Assert.True(result.CreatedAt > OldTimestamp);
        Assert.Null(result.UpdatedAt);

        // Word.ResetId defaults to false independently of the parent's ResetId.
        Assert.Equal(source.Words![0].Id, result.Words![0].Id);
    }

    [Fact]
    public void RegenerateGuid_AssignsNewGuid()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions { RegenerateGuid = true });

        Assert.NotEqual(source.RecordGuid, result.RecordGuid);
    }

    [Fact]
    public void WordResetId_ResetsOnlyWordIdentity()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions
            {
                Word = new WordMappingOptions { ResetId = true },
            });

        Assert.Equal(0, result.Words![0].Id);
        Assert.Equal(0, result.Words![0].UnitId);
        Assert.True(result.Words![0].CreatedAt > OldTimestamp);
        Assert.Null(result.Words![0].UpdatedAt);

        // Siblings and the parent unit are untouched.
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.IrregularForms![0].Id, result.IrregularForms![0].Id);
    }

    [Fact]
    public void WordExcludeExamples_ClearsExamples()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions
            {
                Word = new WordMappingOptions { ExcludeExamples = true },
            });

        Assert.Empty(result.Words![0].Examples!);
    }

    [Fact]
    public void WordExcludeLearningProgress_ResetsRateAndReview()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions
            {
                Word = new WordMappingOptions { ExcludeLearningProgress = true },
            });

        Assert.Equal(3.0f, result.Words![0].Rate);
        Assert.Null(result.Words![0].LastReviewDate);
        Assert.Equal(0, result.Words![0].ReviewCount);
    }

    [Fact]
    public void IrregularFormResetId_ResetsOnlyIrregularFormIdentity()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions
            {
                IrregularForm = new IrregularFormMappingOptions { ResetId = true },
            });

        Assert.Equal(0, result.IrregularForms![0].Id);
        Assert.Equal(0, result.IrregularForms![0].UnitId);
        Assert.Equal(source.Words![0].Id, result.Words![0].Id);
    }

    [Fact]
    public void IrregularFormExcludeLearningProgress_ResetsRateAndReview()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions
            {
                IrregularForm = new IrregularFormMappingOptions { ExcludeLearningProgress = true },
            });

        Assert.Equal(3.0f, result.IrregularForms![0].Rate);
        Assert.Null(result.IrregularForms![0].LastReviewDate);
        Assert.Equal(0, result.IrregularForms![0].ReviewCount);
    }

    [Fact]
    public void StudyCardResetId_ResetsOnlyStudyCardIdentity()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions
            {
                StudyCard = new StudyCardMappingOptions { ResetId = true },
            });

        Assert.Equal(0, result.StudyCards![0].Id);
        Assert.Equal(0, result.StudyCards![0].UnitId);
        Assert.Equal(source.TestCards![0].Id, result.TestCards![0].Id);
    }

    [Fact]
    public void TestCardResetId_ResetsOnlyTestCardIdentity()
    {
        var source = BuildUnit();

        var result = Mapper.Map<UnitModel>(source, opts =>
            opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions
            {
                TestCard = new TestCardMappingOptions { ResetId = true },
            });

        Assert.Equal(0, result.TestCards![0].Id);
        Assert.Equal(0, result.TestCards![0].UnitId);
        Assert.Equal(source.StudyCards![0].Id, result.StudyCards![0].Id);
    }
}
