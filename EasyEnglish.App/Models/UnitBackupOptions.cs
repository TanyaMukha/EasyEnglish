using EasyEnglish.Core.Mapping;

namespace EasyEnglish.Services;

public class UnitBackupOptions
{
    public bool IncludeExamples         { get; set; } = true;
    public bool IncludeLearningProgress { get; set; } = true;
    public bool IsFullBackup            { get; set; } = false;
}

public static class UnitBackupOptionsExtensions
{
    /// <summary>
    /// Converts backup/export options into AutoMapper runtime options.
    /// </summary>
    /// <param name="options">Export/import options.</param>
    /// <param name="isCopy">
    /// True when creating a copy of an existing course —
    /// forces Id reset and Guid regeneration even for full backups.
    /// </param>
    public static UnitMappingOptions ToMappingOptions(
        this UnitBackupOptions options,
        bool isCopy = false)
    {
        bool resetIds = !options.IsFullBackup || isCopy;

        return new UnitMappingOptions
        {
            ResetId        = resetIds,
            RegenerateGuid = isCopy,
            Word = new()
            {
                ResetId                 = resetIds,
                ExcludeExamples         = !options.IncludeExamples,
                ExcludeLearningProgress = !options.IncludeLearningProgress,
            },
            IrregularForm = new()
            {
                ResetId                 = resetIds,
                ExcludeLearningProgress = !options.IncludeLearningProgress,
            },
        };
    }
}
