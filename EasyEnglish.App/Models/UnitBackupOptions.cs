using EasyEnglish.Core.Mapping;

namespace EasyEnglish.Services;

/// <summary>
/// What a course archive carries. Deliberately has no "keep IDs" flag: database IDs are local to one
/// app instance and never survive the trip through an archive — identity travels as
/// <c>RecordGuid</c> only. See EasyEnglish.Docs/Decisions/key-decisions.md.
/// </summary>
public class UnitBackupOptions
{
    public bool IncludeExamples         { get; set; } = true;
    public bool IncludeLearningProgress { get; set; } = true;
}

public static class UnitBackupOptionsExtensions
{
    /// <summary>
    /// Converts backup/export options into AutoMapper runtime options.
    /// <c>ResetId</c> is always on — an archive never carries usable IDs, so the mapped graph starts
    /// at <c>Id == 0</c> everywhere and real IDs are resolved by GUID at import time (see
    /// <c>UnitService.ReconcileAndUpdateAsync</c>).
    /// </summary>
    /// <param name="options">Export/import options.</param>
    /// <param name="isCopy">
    /// True when creating a copy of an existing course — forces GUID regeneration, so the copy is a
    /// genuinely separate course rather than a second claimant to the original's identity.
    /// </param>
    public static UnitMappingOptions ToMappingOptions(
        this UnitBackupOptions options,
        bool isCopy = false)
    {
        return new UnitMappingOptions
        {
            ResetId        = true,
            RegenerateGuid = isCopy,
            Word = new()
            {
                ResetId                 = true,
                ExcludeExamples         = !options.IncludeExamples,
                ExcludeLearningProgress = !options.IncludeLearningProgress,
            },
            IrregularForm = new()
            {
                ResetId                 = true,
                ExcludeLearningProgress = !options.IncludeLearningProgress,
            },
            StudyCard = new()
            {
                ResetId = true,
            },
            TestCard = new()
            {
                ResetId = true,
            },
        };
    }
}
