using System.Text.RegularExpressions;

namespace EasyEnglish.App.Services;

/// <summary>
/// Сервіс для видалення частини текстового рядка, що міститься у квадратних дужках
/// </summary>
public static class TextBracketsRemoverService
{
    private static readonly Regex BracketsPattern = new(@"(?<lead> ?)\[[^\[\]]*\](?<trail> ?)", RegexOptions.Compiled);

    /// <summary>
    /// Видаляє з тексту всі підрядки у квадратних дужках разом із самими дужками.
    /// Якщо одразу після дужки є пробіл — прибирає саме його. Якщо пробілу після
    /// дужки немає (кінець рядка або далі йде символ, що не є буквою чи цифрою,
    /// наприклад крапка чи інша дужка) — замість нього прибирає пробіл перед дужкою,
    /// якщо він там був.
    /// </summary>
    public static string RemoveBracketsText(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var value = text;

        return BracketsPattern.Replace(value, match =>
        {
            var leadingSpace = match.Groups["lead"].Value;

            if (match.Groups["trail"].Length > 0)
                return leadingSpace;

            var nextIndex = match.Index + match.Length;
            var nextIsLetterOrDigit = nextIndex < value.Length && char.IsLetterOrDigit(value[nextIndex]);

            return nextIsLetterOrDigit ? leadingSpace : string.Empty;
        });
    }
}
