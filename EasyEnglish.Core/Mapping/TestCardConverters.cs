using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Models;
using System.Text.Json;

namespace EasyEnglish.Core.Mapping;

/// <summary>
/// The shape in which the Left/Right pair (<see cref="TestCardKind.Matching"/>) is packed into the
/// <c>Options</c> column. Shared by both conversion directions so the property names always match.
/// </summary>
internal sealed class MatchingOptionsJson
{
    public string[] Left { get; set; } = [];
    public string[] Right { get; set; } = [];
}

/// <summary>
/// Розпаковує рядкові JSON-поля Options/CorrectAnswers сутності TestCardEntity
/// у типізовану payload-властивість TestCardModel, обрану за Kind.
/// </summary>
public class TestCardEntityToModelConverter : ITypeConverter<TestCardEntity, TestCardModel>
{
    public TestCardModel Convert(TestCardEntity source, TestCardModel destination, ResolutionContext context)
    {
        destination ??= new TestCardModel();

        destination.Id = source.Id;
        destination.RecordGuid = source.RecordGuid;
        destination.Kind = source.Kind;
        destination.Title = source.Title;
        destination.Question = source.Question;
        destination.Hint = source.Hint;
        destination.Image = source.Image;
        destination.FormattedText = source.FormattedText;
        destination.Explanation = source.Explanation;
        destination.LastReviewDate = source.LastReviewDate;
        destination.ReviewCount = source.ReviewCount;
        destination.Rate = source.Rate;
        destination.CreatedAt = source.CreatedAt;
        destination.UpdatedAt = source.UpdatedAt;
        destination.UnitId = source.UnitId;
        destination.Unit = source.Unit is null ? null : context.Mapper.Map<UnitModel>(source.Unit);

        destination.Choice = null;
        destination.ShortAnswer = null;
        destination.Cloze = null;
        destination.Matching = null;

        switch (source.Kind)
        {
            case TestCardKind.SingleChoice:
            case TestCardKind.MultipleChoice:
                destination.Choice = new ChoicePayload
                {
                    Options = ParseStringArray(source.Options),
                    CorrectAnswers = ParseStringArray(source.CorrectAnswers)
                };
                break;

            case TestCardKind.ShortAnswer:
                destination.ShortAnswer = new ShortAnswerPayload
                {
                    AcceptableAnswers = ParseStringArray(source.CorrectAnswers)
                };
                break;

            case TestCardKind.Cloze:
                destination.Cloze = new ClozePayload
                {
                    CorrectAnswers = string.IsNullOrEmpty(source.CorrectAnswers)
                        ? []
                        : JsonSerializer.Deserialize<string[][]>(source.CorrectAnswers) ?? [],
                    Options = string.IsNullOrEmpty(source.Options)
                        ? null
                        : JsonSerializer.Deserialize<string[][]>(source.Options)
                };
                break;

            case TestCardKind.Matching:
                var matchingOptions = string.IsNullOrEmpty(source.Options)
                    ? null
                    : JsonSerializer.Deserialize<MatchingOptionsJson>(source.Options);
                destination.Matching = new MatchingPayload
                {
                    Left = matchingOptions?.Left ?? [],
                    Right = matchingOptions?.Right ?? [],
                    CorrectRightIndexes = ParseIntArray(source.CorrectAnswers)
                };
                break;
        }

        return destination;
    }

    private static string[] ParseStringArray(string? json) =>
        string.IsNullOrEmpty(json) ? [] : (JsonSerializer.Deserialize<string[]>(json) ?? []);

    private static int[] ParseIntArray(string? json) =>
        string.IsNullOrEmpty(json) ? [] : (JsonSerializer.Deserialize<int[]>(json) ?? []);
}

/// <summary>
/// Пакує типізовану payload-властивість TestCardModel (обрану за Kind)
/// назад у рядкові JSON-поля Options/CorrectAnswers сутності TestCardEntity.
/// </summary>
public class TestCardModelToEntityConverter : ITypeConverter<TestCardModel, TestCardEntity>
{
    public TestCardEntity Convert(TestCardModel source, TestCardEntity destination, ResolutionContext context)
    {
        destination ??= new TestCardEntity();

        destination.Id = source.Id;
        destination.RecordGuid = source.RecordGuid;
        destination.Kind = source.Kind;
        destination.Title = source.Title;
        destination.Question = source.Question;
        destination.Hint = source.Hint;
        destination.Image = source.Image;
        destination.FormattedText = source.FormattedText;
        destination.Explanation = source.Explanation;
        destination.LastReviewDate = source.LastReviewDate;
        destination.ReviewCount = source.ReviewCount;
        destination.Rate = source.Rate;
        destination.CreatedAt = source.CreatedAt;
        destination.UpdatedAt = source.UpdatedAt;
        destination.UnitId = source.UnitId;

        switch (source.Kind)
        {
            case TestCardKind.SingleChoice:
            case TestCardKind.MultipleChoice:
                destination.Options = JsonSerializer.Serialize(source.Choice?.Options ?? []);
                destination.CorrectAnswers = JsonSerializer.Serialize(source.Choice?.CorrectAnswers ?? []);
                break;

            case TestCardKind.ShortAnswer:
                destination.Options = null;
                destination.CorrectAnswers = JsonSerializer.Serialize(source.ShortAnswer?.AcceptableAnswers ?? []);
                break;

            case TestCardKind.Cloze:
                destination.Options = source.Cloze?.Options is null
                    ? null
                    : JsonSerializer.Serialize(source.Cloze.Options);
                destination.CorrectAnswers = JsonSerializer.Serialize(source.Cloze?.CorrectAnswers ?? []);
                break;

            case TestCardKind.Matching:
                destination.Options = JsonSerializer.Serialize(new MatchingOptionsJson
                {
                    Left = source.Matching?.Left ?? [],
                    Right = source.Matching?.Right ?? []
                });
                destination.CorrectAnswers = JsonSerializer.Serialize(source.Matching?.CorrectRightIndexes ?? []);
                break;

            default:
                destination.Options = null;
                destination.CorrectAnswers = null;
                break;
        }

        return destination;
    }
}
