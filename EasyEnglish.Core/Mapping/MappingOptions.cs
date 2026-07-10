namespace EasyEnglish.Core.Mapping;

public class WordMappingOptions
{
    public bool ResetId { get; init; }           // default: зберігати Id
    public bool ExcludeExamples { get; init; }   // default: включати
    public bool ExcludeLearningProgress { get; init; } // default: включати
}

public class IrregularFormMappingOptions
{
    public bool ResetId { get; init; }
    public bool ExcludeLearningProgress { get; init; }
}

public class StudyCardMappingOptions
{
    public bool ResetId { get; init; }
}

public class TestCardMappingOptions
{
    public bool ResetId { get; init; }
}

public class UnitMappingOptions
{
    public const string Key = nameof(UnitMappingOptions);

    public bool ResetId { get; init; }
    public bool RegenerateGuid { get; init; }

    public WordMappingOptions Word { get; init; } = new();
    public IrregularFormMappingOptions IrregularForm { get; init; } = new();
    public StudyCardMappingOptions StudyCard { get; init; } = new();
    public TestCardMappingOptions TestCard { get; init; } = new();
}
