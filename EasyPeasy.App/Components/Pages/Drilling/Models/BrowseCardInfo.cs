namespace EasyPeasy.App.Components.Pages.Drilling.Models;

/// <summary>
/// One row of the browse-mode card list (the bottom sheet with all cards).
/// </summary>
/// <param name="Index">Position in the browse queue — passed back when jumping to the card.</param>
/// <param name="Label">Short caption (word, form, sentence, card title).</param>
/// <param name="Rating">Current difficulty rating, if the item has one — drives the colour dot.</param>
/// <param name="IsVisited">Whether the learner has opened this card during the session.</param>
public record BrowseCardInfo(int Index, string Label, double? Rating, bool IsVisited);
