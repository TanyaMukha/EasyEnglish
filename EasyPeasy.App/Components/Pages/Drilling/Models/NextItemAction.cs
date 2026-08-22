namespace EasyPeasy.App.Components.Pages.Drilling.Models;

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
