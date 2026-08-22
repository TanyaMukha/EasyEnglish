namespace EasyPeasy.Core.Mapping;

/// <summary>Word-level options nested under <see cref="UnitMappingOptions"/>. All flags default to <c>false</c> (keep everything as-is).</summary>
public class WordMappingOptions
{
    /// <summary>When <c>true</c>, resets <c>Id</c>/<c>UnitId</c> to 0 and refreshes timestamps (for cloning as a new record).</summary>
    public bool ResetId { get; init; }

    /// <summary>When <c>true</c>, drops the word's <see cref="EasyPeasy.Core.Models.WordModel.Examples"/>.</summary>
    public bool ExcludeExamples { get; init; }

    /// <summary>When <c>true</c>, resets <c>Rate</c>/<c>LastReviewDate</c>/<c>ReviewCount</c> to their defaults.</summary>
    public bool ExcludeLearningProgress { get; init; }
}

/// <summary>Irregular-form-level options nested under <see cref="UnitMappingOptions"/>. All flags default to <c>false</c>.</summary>
public class IrregularFormMappingOptions
{
    /// <summary>When <c>true</c>, resets <c>Id</c>/<c>UnitId</c> to 0 and refreshes timestamps.</summary>
    public bool ResetId { get; init; }

    /// <summary>When <c>true</c>, resets <c>Rate</c>/<c>LastReviewDate</c>/<c>ReviewCount</c> to their defaults.</summary>
    public bool ExcludeLearningProgress { get; init; }
}

/// <summary>Study-card-level options nested under <see cref="UnitMappingOptions"/>.</summary>
public class StudyCardMappingOptions
{
    /// <summary>When <c>true</c>, resets <c>Id</c>/<c>UnitId</c> to 0 and refreshes timestamps.</summary>
    public bool ResetId { get; init; }
}

/// <summary>Test-card-level options nested under <see cref="UnitMappingOptions"/>.</summary>
public class TestCardMappingOptions
{
    /// <summary>When <c>true</c>, resets <c>Id</c>/<c>UnitId</c> to 0 and refreshes timestamps.</summary>
    public bool ResetId { get; init; }
}

/// <summary>
/// Options controlling a <c>UnitModel → UnitModel</c> self-map (see <see cref="EasyPeasy.Core.Mapping.UnitMappingAction"/>),
/// passed via <c>mapper.Map(unit, opts => opts.Items[UnitMappingOptions.Key] = new UnitMappingOptions { ... })</c>.
/// Note: this only affects <see cref="EasyPeasy.Core.Models.UnitModel.Words"/> and their nested
/// <see cref="EasyPeasy.Core.Models.ExampleModel"/> — <c>IrregularForm</c>/<c>StudyCard</c>/<c>TestCard</c>
/// options below exist but are wired to their respective mapping actions independently.
/// </summary>
public class UnitMappingOptions
{
    /// <summary>The key under which an instance of this class must be stored in <c>ResolutionContext.Items</c>.</summary>
    public const string Key = nameof(UnitMappingOptions);

    /// <summary>When <c>true</c>, resets <c>Id</c>/<c>CourseId</c> to 0 and refreshes timestamps.</summary>
    public bool ResetId { get; init; }

    /// <summary>When <c>true</c>, assigns a new <see cref="EasyPeasy.Core.Interfaces.Fields.IGuidInfo.RecordGuid"/>.</summary>
    public bool RegenerateGuid { get; init; }

    public WordMappingOptions Word { get; init; } = new();
    public IrregularFormMappingOptions IrregularForm { get; init; } = new();
    public StudyCardMappingOptions StudyCard { get; init; } = new();
    public TestCardMappingOptions TestCard { get; init; } = new();
}
