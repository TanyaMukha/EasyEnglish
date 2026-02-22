using System.Text.RegularExpressions;

namespace EasyEnglish.App.Services;

/// <summary>
/// Сервіс для роботи з прикладами, що містять markdown та прихований текст
/// </summary>
public static class ExampleMarkdownService
{
    /// <summary>
    /// Типи markdown маркерів, які можуть бути прихованим текстом
    /// </summary>
    public enum HiddenTextMarker
    {
        None,           // Немає прихованого тексту
        Bold,           // **text**
        Italic,         // __text__
        BoldItalic,     // ***text***
        Code            // `text`
    }

    private static readonly Dictionary<HiddenTextMarker, string> MarkerPatterns = new()
    {
        { HiddenTextMarker.Bold, @"\*\*(.+?)\*\*" },
        { HiddenTextMarker.Italic, @"__(.+?)__" },
        { HiddenTextMarker.BoldItalic, @"\*\*\*(.+?)\*\*\*" },
        { HiddenTextMarker.Code, @"`(.+?)`" }
    };

    /// <summary>
    /// Парсить речення та знаходить приховану частину
    /// </summary>
    public static (string beforeHidden, string hiddenText, string afterHidden) ParseHiddenText(
        string sentence,
        HiddenTextMarker marker = HiddenTextMarker.Bold)
    {
        if (marker == HiddenTextMarker.None || !MarkerPatterns.ContainsKey(marker))
            return (sentence, "", "");

        var pattern = MarkerPatterns[marker];
        var match = Regex.Match(sentence, pattern);

        if (match.Success)
        {
            var beforeIndex = match.Index;
            var afterIndex = match.Index + match.Length;

            var before = sentence.Substring(0, beforeIndex);
            var hidden = match.Groups[1].Value;
            var after = sentence.Substring(afterIndex);

            return (before, hidden, after);
        }

        return (sentence, "", "");
    }

    /// <summary>
    /// Перевіряє чи містить речення приховану частину
    /// </summary>
    public static bool HasHiddenText(string sentence, HiddenTextMarker marker = HiddenTextMarker.Bold)
    {
        if (marker == HiddenTextMarker.None || !MarkerPatterns.ContainsKey(marker))
            return false;

        var pattern = MarkerPatterns[marker];
        return Regex.IsMatch(sentence, pattern);
    }

    /// <summary>
    /// Рендерить markdown (окрім вказаного маркера, якщо він прихований)
    /// </summary>
    public static string RenderMarkdown(
        string text,
        HiddenTextMarker hiddenMarker = HiddenTextMarker.None)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var html = text;

        // Жирний наклонний: ***text*** (обробляємо першим!)
        if (hiddenMarker != HiddenTextMarker.BoldItalic)
        {
            html = Regex.Replace(html, @"\*\*\*(.+?)\*\*\*", "<strong><em>$1</em></strong>");
        }

        // Жирний: **text**
        if (hiddenMarker != HiddenTextMarker.Bold)
        {
            html = Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        }

        // Наклонний: __text__
        if (hiddenMarker != HiddenTextMarker.Italic)
        {
            html = Regex.Replace(html, @"__(.+?)__", "<em>$1</em>");
        }

        // Код: `text`
        if (hiddenMarker != HiddenTextMarker.Code)
        {
            html = Regex.Replace(html, @"`(.+?)`", "<code>$1</code>");
        }

        return html;
    }

    /// <summary>
    /// Рендерить markdown включаючи приховану частину як span з класом
    /// </summary>
    public static string RenderMarkdownWithHidden(
        string sentence,
        bool showHidden = false,
        HiddenTextMarker hiddenMarker = HiddenTextMarker.Bold)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return string.Empty;

        var html = sentence;

        // Обробляємо приховану частину
        if (hiddenMarker != HiddenTextMarker.None && MarkerPatterns.ContainsKey(hiddenMarker))
        {
            var pattern = MarkerPatterns[hiddenMarker];

            if (showHidden)
            {
                html = Regex.Replace(
                    html,
                    pattern,
                    "<span class='hidden-text revealed'>$1</span>"
                );
            }
            else
            {
                html = Regex.Replace(
                    html,
                    pattern,
                    "<span class='hidden-text blurred'>$1</span>"
                );
            }
        }

        // Потім інші markdown елементи
        html = RenderMarkdown(html, hiddenMarker);

        return html;
    }

    /// <summary>
    /// Видаляє всі markdown маркери і повертає чистий текст
    /// </summary>
    public static string StripMarkdown(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var clean = text;

        // Видаляємо всі markdown маркери
        clean = Regex.Replace(clean, @"\*\*\*(.+?)\*\*\*", "$1"); // ***text***
        clean = Regex.Replace(clean, @"\*\*(.+?)\*\*", "$1");     // **text**
        clean = Regex.Replace(clean, @"__(.+?)__", "$1");         // __text__
        clean = Regex.Replace(clean, @"`(.+?)`", "$1");           // `text`

        return clean;
    }

    /// <summary>
    /// Отримує тільки приховану частину без markdown
    /// </summary>
    public static string GetHiddenTextOnly(string sentence, HiddenTextMarker marker = HiddenTextMarker.Bold)
    {
        if (marker == HiddenTextMarker.None || !MarkerPatterns.ContainsKey(marker))
            return "";

        var pattern = MarkerPatterns[marker];
        var match = Regex.Match(sentence, pattern);
        return match.Success ? match.Groups[1].Value : "";
    }

    /// <summary>
    /// Перевіряє відповідь користувача (ігноруючи регістр та пробіли)
    /// </summary>
    public static bool CheckAnswer(string userAnswer, string correctAnswer)
    {
        var userClean = userAnswer.Trim().ToLowerInvariant();
        var correctClean = correctAnswer.Trim().ToLowerInvariant();

        return userClean == correctClean;
    }
}