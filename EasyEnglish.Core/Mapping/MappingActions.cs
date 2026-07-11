using AutoMapper;
using EasyEnglish.Core.Models;

namespace EasyEnglish.Core.Mapping;

/// <summary>
/// Post-processes a <c>UnitModel → UnitModel</c> self-map, applying <see cref="UnitMappingOptions"/>
/// passed via <c>ResolutionContext.Items[UnitMappingOptions.Key]</c> (see <see cref="MappingContextExtensions.GetUnitOptions"/>).
/// Used for cloning a unit (e.g. import/export/duplication) where the caller wants to reset identity
/// fields and/or learning progress instead of a literal deep copy.
/// </summary>
public class UnitMappingAction : IMappingAction<UnitModel, UnitModel>
{
    public void Process(UnitModel source, UnitModel destination, ResolutionContext context)
    {
        var opts = context.GetUnitOptions();
        if (opts is null) return; // no options supplied — leave the plain copy as-is

        if (opts.ResetId)
        {
            destination.Id = 0;
            destination.CourseId = 0;
            destination.UpdatedAt = null;
            destination.CreatedAt = DateTime.UtcNow;
        }

        if (opts.RegenerateGuid)
            destination.RecordGuid = Guid.NewGuid();
    }
}

/// <summary>
/// Post-processes a <c>WordModel → WordModel</c> self-map, applying <see cref="WordMappingOptions"/>
/// nested under the ambient <see cref="UnitMappingOptions"/> (see <see cref="UnitMappingAction"/>).
/// A no-op when no <see cref="UnitMappingOptions"/> is in the mapping context.
/// </summary>
public class WordMappingAction : IMappingAction<WordModel, WordModel>
{
    public void Process(WordModel source, WordModel destination, ResolutionContext context)
    {
        var opts = context.GetUnitOptions()?.Word;
        if (opts is null) return;

        if (opts.ResetId)
        {
            destination.Id = 0;
            destination.UnitId = 0;
            destination.CreatedAt = DateTime.UtcNow;
            destination.UpdatedAt = null;
        }

        if (opts.ExcludeLearningProgress)
        {
            destination.Rate = 3.0f;
            destination.LastReviewDate = null;
            destination.ReviewCount = 0;
        }

        if (opts.ExcludeExamples)
            destination.Examples = [];
    }
}

/// <summary>
/// Post-processes an <c>ExampleModel → ExampleModel</c> self-map, applying the same
/// <see cref="WordMappingOptions"/> as <see cref="WordMappingAction"/> (examples share their
/// parent word's <c>ResetId</c> flag, since they're always cloned together).
/// </summary>
public class ExampleMappingAction : IMappingAction<ExampleModel, ExampleModel>
{
    public void Process(ExampleModel source, ExampleModel destination, ResolutionContext context)
    {
        var opts = context.GetUnitOptions()?.Word;
        if (opts is null) return;

        if (opts.ResetId)
        {
            destination.Id = 0;
            destination.WordId = 0;
        }
    }
}

/// <summary>
/// Post-processes an <c>IrregularFormModel → IrregularFormModel</c> self-map, applying
/// <see cref="IrregularFormMappingOptions"/> nested under the ambient <see cref="UnitMappingOptions"/>.
/// </summary>
public class IrregularFormMappingAction : IMappingAction<IrregularFormModel, IrregularFormModel>
{
    public void Process(IrregularFormModel source, IrregularFormModel destination, ResolutionContext context)
    {
        var opts = context.GetUnitOptions()?.IrregularForm;
        if (opts is null) return;

        if (opts.ResetId)
        {
            destination.Id = 0;
            destination.UnitId = 0;
            destination.CreatedAt = DateTime.UtcNow;
            destination.UpdatedAt = null;
        }

        if (opts.ExcludeLearningProgress)
        {
            destination.Rate = 3.0f;
            destination.LastReviewDate = null;
            destination.ReviewCount = 0;
        }
    }
}

/// <summary>
/// Post-processes a <c>StudyCardModel → StudyCardModel</c> self-map, applying
/// <see cref="StudyCardMappingOptions"/> nested under the ambient <see cref="UnitMappingOptions"/>.
/// </summary>
public class StudyCardMappingAction : IMappingAction<StudyCardModel, StudyCardModel>
{
    public void Process(StudyCardModel source, StudyCardModel destination, ResolutionContext context)
    {
        var opts = context.GetUnitOptions()?.StudyCard;
        if (opts is null) return;

        if (opts.ResetId)
        {
            destination.Id = 0;
            destination.UnitId = 0;
            destination.CreatedAt = DateTime.UtcNow;
            destination.UpdatedAt = null;
        }
    }
}

/// <summary>
/// Post-processes a <c>TestCardModel → TestCardModel</c> self-map, applying
/// <see cref="TestCardMappingOptions"/> nested under the ambient <see cref="UnitMappingOptions"/>.
/// </summary>
public class TestCardMappingAction : IMappingAction<TestCardModel, TestCardModel>
{
    public void Process(TestCardModel source, TestCardModel destination, ResolutionContext context)
    {
        var opts = context.GetUnitOptions()?.TestCard;
        if (opts is null) return;

        if (opts.ResetId)
        {
            destination.Id = 0;
            destination.UnitId = 0;
            destination.CreatedAt = DateTime.UtcNow;
            destination.UpdatedAt = null;
        }
    }
}
