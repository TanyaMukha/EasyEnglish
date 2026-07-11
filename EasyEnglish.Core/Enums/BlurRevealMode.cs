namespace EasyEnglish.Core.Enums;

/// <summary>
/// How blurred segments reveal in a <see cref="StudyCardKind.BlurredText"/> card.
/// </summary>
public enum BlurRevealMode
{
    /// <summary>Each blurred segment reveals on its own click, independent of the others.</summary>
    Independent = 0,

    /// <summary>Clicking any blurred segment reveals all of them at once (e.g. for phrasal verbs like "look up").</summary>
    Grouped = 1
}
