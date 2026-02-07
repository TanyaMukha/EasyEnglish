using System.Text.RegularExpressions;

public static class CustomMarkdownRenderer
{
    public static string RenderCustomMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var html = markdown;

        // Жирний наклонний: ***text*** (обробляємо першим!)
        html = Regex.Replace(
            html,
            @"\*\*\*(.+?)\*\*\*",
            "<strong><em>$1</em></strong>"
        );

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

        // Посилання: [text](url)
        html = Regex.Replace(
            html,
            @"\[(.+?)\]\((.+?)\)",
            "<a href=\"$2\">$1</a>"
        );

        // Переноси рядків
        html = html.Replace("\n", "<br />");

        return html;
    }
}

// Використання у Razor компоненті:
// @((MarkupString)CustomMarkdownRenderer.RenderCustomMarkdown(feedbackMessage ?? ""))

/* Приклади використання:
 * 
 * **жирний текст**           → <strong>жирний текст</strong>
 * __наклонний текст__        → <em>наклонний текст</em>
 * ***жирний наклонний***     → <strong><em>жирний наклонний</em></strong>
 * 
 * У коді:
 * feedbackMessage = $"Incorrect. Your answer: **{userInput}**";     // жирний
 * feedbackMessage = $"Incorrect. Your answer: __{userInput}__";     // наклонний
 * feedbackMessage = $"Incorrect. Your answer: ***{userInput}***";   // жирний наклонний
 */