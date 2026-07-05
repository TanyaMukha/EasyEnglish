namespace EasyEnglish.Core.Enums;

/// <summary>
/// Вид навчальної (неоцінюваної) картки модуля.
/// </summary>
public enum StudyCardKind
{
    /// <summary>
    /// Термін і його визначення.
    /// </summary>
    Term = 0,

    /// <summary>
    /// Короткий текст, можливо з блоком коду чи діалогом.
    /// </summary>
    Text = 1,

    /// <summary>
    /// Текст із розмитими фрагментами, які відкриваються по кліку.
    /// </summary>
    BlurredText = 2
}
