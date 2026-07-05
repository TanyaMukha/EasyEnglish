namespace EasyEnglish.Core.Enums;

/// <summary>
/// Спосіб розкриття розмитих фрагментів у картці виду <see cref="StudyCardKind.BlurredText"/>.
/// </summary>
public enum BlurRevealMode
{
    /// <summary>
    /// Кожен розмитий фрагмент відкривається своїм кліком незалежно від інших.
    /// </summary>
    Independent = 0,

    /// <summary>
    /// Клік по будь-якому розмитому фрагменту відкриває одразу всі
    /// (наприклад, для фразових дієслів на кшталт "look up").
    /// </summary>
    Grouped = 1
}
