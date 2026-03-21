using Microsoft.AspNetCore.Components;

namespace EasyEnglish.App.Components.Pages.Drilling.Models;

/// <summary>
/// What to do with the current item after "Next" is clicked.
/// </summary>
public enum NextItemAction
{
    /// <summary>Item completed — remove from queue.</summary>
    Remove,

    /// <summary>Wrong answer — reinsert later in the queue.</summary>
    Requeue
}

/// <summary>
/// All event callbacks the test engine exposes to card components.
/// Картка сама визначає правильність і надсилає тільки фінальний bool через OnCheckAnswer.
/// </summary>
public record TestCallbacks(
    EventCallback<bool>   OnCheckAnswer,   // картка надсилає true/false — фінальний результат
    EventCallback         OnReveal,        // blurred
    EventCallback<double> OnRate           // difficulty rating
);
