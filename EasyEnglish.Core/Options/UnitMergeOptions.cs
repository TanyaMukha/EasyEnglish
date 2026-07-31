namespace EasyEnglish.Core.Options;

/// <summary>
/// How an incoming unit graph's learning progress (<c>Rate</c>/<c>LastReviewDate</c>/<c>ReviewCount</c>)
/// is combined with what's already stored, for children matched by <c>RecordGuid</c>.
/// </summary>
public enum LearningProgressMerge
{
    /// <summary>
    /// Ignore the incoming progress entirely and keep what's in the database. Use when the payload
    /// doesn't actually carry progress — a course archive exported without it still contains
    /// *default* values (rate 3.0, never reviewed), which would otherwise silently wipe real
    /// progress on update.
    /// </summary>
    KeepExisting,

    /// <summary>
    /// Per item, keep whichever side was reviewed more recently (by <c>LastReviewDate</c>; a
    /// never-reviewed item counts as oldest). Lets the same course be studied on two devices and
    /// synced in either direction without progress ever rolling backwards.
    /// </summary>
    PreferNewest,
}

/// <summary>
/// Describes what an incoming unit graph is authoritative for, so a <em>partial</em> payload (a
/// course archive exported without examples or without learning progress) can still be merged
/// without destroying the data it simply doesn't carry.
/// <para>
/// Identity is always resolved by <c>RecordGuid</c>, never by <c>Id</c> — database IDs aren't
/// portable between app instances. See EasyEnglish.Docs/Decisions/key-decisions.md.
/// </para>
/// </summary>
public class UnitMergeOptions
{
    /// <summary>
    /// When <c>true</c>, children whose <c>RecordGuid</c> isn't in the incoming graph are deleted
    /// (strict sync). Only ever applied to collections the payload actually carries — with
    /// <see cref="MergeExamples"/> <c>false</c>, examples are never deleted no matter what this says.
    /// </summary>
    public bool DeleteMissing { get; init; }

    /// <summary>How to combine learning progress for children matched by <c>RecordGuid</c>.</summary>
    public LearningProgressMerge LearningProgress { get; init; } = LearningProgressMerge.PreferNewest;

    /// <summary>
    /// When <c>false</c>, every matched word keeps its stored examples untouched and the incoming
    /// ones are ignored — for payloads exported without examples, where an empty list means "not
    /// included" rather than "the user deleted them all".
    /// </summary>
    public bool MergeExamples { get; init; } = true;
}
