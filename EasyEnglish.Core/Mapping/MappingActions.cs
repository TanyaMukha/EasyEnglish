using AutoMapper;
using EasyEnglish.Core.Models;

namespace EasyEnglish.Core.Mapping;

public class UnitMappingAction : IMappingAction<UnitModel, UnitModel>
{
    public void Process(UnitModel source, UnitModel destination, ResolutionContext context)
    {
        var opts = context.GetUnitOptions();
        if (opts is null) return; // ← без опцій — нічого не чіпаємо

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
