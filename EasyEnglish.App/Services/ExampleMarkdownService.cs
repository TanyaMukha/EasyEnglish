using System.Text.RegularExpressions;

namespace EasyEnglish.App.Services;

/// <summary>
/// Сервіс для роботи з прикладами, що містять markdown та прихований текст
/// </summary>
public static class ExampleMarkdownService
{
    /// <summary>
    /// Парсить речення та знаходить приховану частину (виділену ***text***)
    /// </summary>
    public static (string beforeHidden, string hiddenText, string afterHidden) ParseHiddenText(string sentence)
    {
        // Шукаємо текст в потрійних зірочках ***text***
        var match = Regex.Match(sentence, @"\*\*\*(.+?)\*\*\*");
        
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
    public static bool HasHiddenText(string sentence)
    {
        return Regex.IsMatch(sentence, @"\*\*\*(.+?)\*\*\*");
    }
    
    /// <summary>
    /// Рендерить markdown (окрім прихованого тексту ***text***)
    /// </summary>
    public static string RenderMarkdown(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var html = text;

        // Жирний: **text**
        html = Regex.Replace(
            html,
            @"\*\*(.+?)\*\*",
            "<strong>$1</strong>"
        );

        // Наклонний: __text__
        html = Regex.Replace(
            html,
            @"__(.+?)__",
            "<em>$1</em>"
        );

        // Код: `text`
        html = Regex.Replace(
            html,
            @"`(.+?)`",
            "<code>$1</code>"
        );

        return html;
    }
    
    /// <summary>
    /// Рендерить markdown включаючи приховану частину як span з класом
    /// </summary>
    public static string RenderMarkdownWithHidden(string sentence, bool showHidden = false)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return string.Empty;

        var html = sentence;
        
        // Обробляємо приховану частину ***text***
        if (showHidden)
        {
            // Показуємо як виділений текст
            html = Regex.Replace(
                html,
                @"\*\*\*(.+?)\*\*\*",
                "<span class='hidden-text revealed'>$1</span>"
            );
        }
        else
        {
            // Залишаємо як маркер для обробки в компоненті
            html = Regex.Replace(
                html,
                @"\*\*\*(.+?)\*\*\*",
                "<span class='hidden-text blurred'>$1</span>"
            );
        }
        
        // Потім інші markdown елементи
        html = RenderMarkdown(html);
        
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
    public static string GetHiddenTextOnly(string sentence)
    {
        var match = Regex.Match(sentence, @"\*\*\*(.+?)\*\*\*");
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
