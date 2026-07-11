using System.Text.Json;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Tests.Fixtures;

namespace EasyEnglish.Core.Tests;

/// <summary>
/// Round-trip tests for <c>TestCardEntityToModelConverter</c>/<c>TestCardModelToEntityConverter</c> —
/// the highest-risk logic in EasyEnglish.Core (hand-written JSON pack/unpack per <see cref="TestCardKind"/>,
/// with no exception thrown on a wrong mapping — just silently wrong quiz options).
/// </summary>
public class TestCardConvertersTests
{
    private static readonly AutoMapper.IMapper Mapper = MapperFactory.Instance;

    [Fact]
    public void EntityToModel_SingleChoice_PopulatesChoicePayloadOnly()
    {
        var entity = new TestCardEntity
        {
            Kind = TestCardKind.SingleChoice,
            Options = JsonSerializer.Serialize(new[] { "a", "b", "c" }),
            CorrectAnswers = JsonSerializer.Serialize(new[] { "b" }),
        };

        var model = Mapper.Map<TestCardModel>(entity);

        Assert.NotNull(model.Choice);
        Assert.Equal(new[] { "a", "b", "c" }, model.Choice!.Options);
        Assert.Equal(new[] { "b" }, model.Choice!.CorrectAnswers);
        Assert.Null(model.ShortAnswer);
        Assert.Null(model.Cloze);
        Assert.Null(model.Matching);
    }

    [Fact]
    public void EntityToModel_MultipleChoice_AllowsSeveralCorrectAnswers()
    {
        var entity = new TestCardEntity
        {
            Kind = TestCardKind.MultipleChoice,
            Options = JsonSerializer.Serialize(new[] { "a", "b", "c" }),
            CorrectAnswers = JsonSerializer.Serialize(new[] { "a", "c" }),
        };

        var model = Mapper.Map<TestCardModel>(entity);

        Assert.Equal(new[] { "a", "c" }, model.Choice!.CorrectAnswers);
    }

    [Fact]
    public void RoundTrip_Choice_PreservesOptionsAndCorrectAnswers()
    {
        var original = new TestCardEntity
        {
            Kind = TestCardKind.SingleChoice,
            Options = JsonSerializer.Serialize(new[] { "a", "b", "c" }),
            CorrectAnswers = JsonSerializer.Serialize(new[] { "b" }),
        };

        var roundTripped = Mapper.Map<TestCardEntity>(Mapper.Map<TestCardModel>(original));

        Assert.Equal(original.Options, roundTripped.Options);
        Assert.Equal(original.CorrectAnswers, roundTripped.CorrectAnswers);
    }

    [Fact]
    public void EntityToModel_ShortAnswer_PopulatesAcceptableAnswersAndNothingElse()
    {
        var entity = new TestCardEntity
        {
            Kind = TestCardKind.ShortAnswer,
            Options = null,
            CorrectAnswers = JsonSerializer.Serialize(new[] { "answer1", "answer2" }),
        };

        var model = Mapper.Map<TestCardModel>(entity);

        Assert.NotNull(model.ShortAnswer);
        Assert.Equal(new[] { "answer1", "answer2" }, model.ShortAnswer!.AcceptableAnswers);
        Assert.Null(model.Choice);
        Assert.Null(model.Cloze);
        Assert.Null(model.Matching);
    }

    [Fact]
    public void RoundTrip_ShortAnswer_OptionsStaysNull()
    {
        var original = new TestCardEntity
        {
            Kind = TestCardKind.ShortAnswer,
            Options = null,
            CorrectAnswers = JsonSerializer.Serialize(new[] { "answer1" }),
        };

        var roundTripped = Mapper.Map<TestCardEntity>(Mapper.Map<TestCardModel>(original));

        Assert.Null(roundTripped.Options);
        Assert.Equal(original.CorrectAnswers, roundTripped.CorrectAnswers);
    }

    [Fact]
    public void EntityToModel_Cloze_NullOptions_MeansAllBlanksAreFreeText()
    {
        var entity = new TestCardEntity
        {
            Kind = TestCardKind.Cloze,
            Options = null,
            CorrectAnswers = JsonSerializer.Serialize(new[] { new[] { "ans1" }, new[] { "ans2", "ans2b" } }),
        };

        var model = Mapper.Map<TestCardModel>(entity);

        Assert.NotNull(model.Cloze);
        Assert.Null(model.Cloze!.Options);
        Assert.Equal(2, model.Cloze!.CorrectAnswers.Length);
        Assert.Equal(new[] { "ans1" }, model.Cloze!.CorrectAnswers[0]);
        Assert.Equal(new[] { "ans2", "ans2b" }, model.Cloze!.CorrectAnswers[1]);
    }

    [Fact]
    public void EntityToModel_Cloze_MixedOptions_NullElementIsFreeTextNonNullIsDropdown()
    {
        // Position 0: null -> free-text input. Position 1: non-empty array -> dropdown.
        var optionsJson = JsonSerializer.Serialize(new[] { null, new[] { "x", "y" } });
        var entity = new TestCardEntity
        {
            Kind = TestCardKind.Cloze,
            Options = optionsJson,
            CorrectAnswers = JsonSerializer.Serialize(new[] { new[] { "x" }, new[] { "y" } }),
        };

        var model = Mapper.Map<TestCardModel>(entity);

        Assert.NotNull(model.Cloze!.Options);
        Assert.Equal(2, model.Cloze!.Options!.Length);
        Assert.Null(model.Cloze!.Options![0]);
        Assert.Equal(new[] { "x", "y" }, model.Cloze!.Options![1]);
    }

    [Fact]
    public void RoundTrip_Cloze_NullOptionsDoesNotBecomeEmptyArray()
    {
        // The whole point of this test: entity.Options == null must round-trip as null,
        // never as the JSON literal "[]" (those mean different things: no dropdown data at all
        // vs. an explicit empty set of blanks).
        var original = new TestCardEntity
        {
            Kind = TestCardKind.Cloze,
            Options = null,
            CorrectAnswers = JsonSerializer.Serialize(new[] { new[] { "ans1" } }),
        };

        var roundTripped = Mapper.Map<TestCardEntity>(Mapper.Map<TestCardModel>(original));

        Assert.Null(roundTripped.Options);
        Assert.Equal(original.CorrectAnswers, roundTripped.CorrectAnswers);
    }

    [Fact]
    public void RoundTrip_Cloze_NonNullOptionsPreserved()
    {
        var original = new TestCardEntity
        {
            Kind = TestCardKind.Cloze,
            Options = JsonSerializer.Serialize(new[] { null, new[] { "x", "y" } }),
            CorrectAnswers = JsonSerializer.Serialize(new[] { new[] { "x" }, new[] { "y" } }),
        };

        var roundTripped = Mapper.Map<TestCardEntity>(Mapper.Map<TestCardModel>(original));

        Assert.Equal(original.Options, roundTripped.Options);
        Assert.Equal(original.CorrectAnswers, roundTripped.CorrectAnswers);
    }

    [Fact]
    public void EntityToModel_Matching_PopulatesLeftRightAndCorrectIndexes()
    {
        var entity = new TestCardEntity
        {
            Kind = TestCardKind.Matching,
            Options = JsonSerializer.Serialize(new { Left = new[] { "a", "b" }, Right = new[] { "1", "2", "3" } }),
            CorrectAnswers = JsonSerializer.Serialize(new[] { 1, 0 }),
        };

        var model = Mapper.Map<TestCardModel>(entity);

        Assert.NotNull(model.Matching);
        Assert.Equal(new[] { "a", "b" }, model.Matching!.Left);
        Assert.Equal(new[] { "1", "2", "3" }, model.Matching!.Right);
        Assert.Equal(new[] { 1, 0 }, model.Matching!.CorrectRightIndexes);
    }

    [Fact]
    public void RoundTrip_Matching_PreservesLeftRightAndCorrectIndexes()
    {
        var original = new TestCardEntity
        {
            Kind = TestCardKind.Matching,
            Options = JsonSerializer.Serialize(new { Left = new[] { "a", "b" }, Right = new[] { "1", "2", "3" } }),
            CorrectAnswers = JsonSerializer.Serialize(new[] { 1, 0 }),
        };

        var roundTripped = Mapper.Map<TestCardEntity>(Mapper.Map<TestCardModel>(original));

        var originalModel = Mapper.Map<TestCardModel>(original);
        var roundTrippedModel = Mapper.Map<TestCardModel>(roundTripped);

        Assert.Equal(originalModel.Matching!.Left, roundTrippedModel.Matching!.Left);
        Assert.Equal(originalModel.Matching!.Right, roundTrippedModel.Matching!.Right);
        Assert.Equal(originalModel.Matching!.CorrectRightIndexes, roundTrippedModel.Matching!.CorrectRightIndexes);
    }

    [Fact]
    public void ModelToEntity_UnrecognizedKind_ClearsOptionsAndCorrectAnswers()
    {
        // Kind is a plain enum with no range restriction — an out-of-range value takes the
        // converter's `default` branch, distinct from every named TestCardKind case.
        var model = new TestCardModel
        {
            Kind = (TestCardKind)99,
            Choice = new ChoicePayload { Options = ["should", "be", "dropped"] },
        };

        var entity = Mapper.Map<TestCardEntity>(model);

        Assert.Null(entity.Options);
        Assert.Null(entity.CorrectAnswers);
    }

    [Fact]
    public void EntityToModel_MapsSharedFieldsRegardlessOfKind()
    {
        var entity = new TestCardEntity
        {
            Id = 42,
            RecordGuid = Guid.NewGuid(),
            Kind = TestCardKind.ShortAnswer,
            Title = "Title",
            Question = "Question?",
            Hint = "Hint",
            Explanation = "Explanation",
            FormattedText = "Formatted",
            Rate = 4.5f,
            ReviewCount = 3,
            UnitId = 7,
            CorrectAnswers = JsonSerializer.Serialize(new[] { "x" }),
        };

        var model = Mapper.Map<TestCardModel>(entity);

        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.RecordGuid, model.RecordGuid);
        Assert.Equal(entity.Title, model.Title);
        Assert.Equal(entity.Question, model.Question);
        Assert.Equal(entity.Hint, model.Hint);
        Assert.Equal(entity.Explanation, model.Explanation);
        Assert.Equal(entity.FormattedText, model.FormattedText);
        Assert.Equal(entity.Rate, model.Rate);
        Assert.Equal(entity.ReviewCount, model.ReviewCount);
        Assert.Equal(entity.UnitId, model.UnitId);
    }
}
