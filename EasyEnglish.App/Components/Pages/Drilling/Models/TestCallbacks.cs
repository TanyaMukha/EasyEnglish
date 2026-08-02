using Microsoft.AspNetCore.Components;

namespace EasyEnglish.App.Components.Pages.Drilling.Models;

/// <summary>
/// All event callbacks the test engine exposes to card components.
/// The card decides correctness itself and reports only the final bool through OnCheckAnswer.
/// </summary>
public record TestCallbacks(
    EventCallback<bool>   OnCheckAnswer,   // the card sends true/false — the final result
    EventCallback         OnReveal,        // blurred
    EventCallback<double> OnRate           // difficulty rating
);
