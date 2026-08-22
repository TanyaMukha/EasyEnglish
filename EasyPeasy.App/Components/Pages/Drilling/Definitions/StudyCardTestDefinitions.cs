using EasyPeasy.App.Models;
using EasyPeasy.App.Components.Pages.Drilling.Models;
using ReviewStudyCard = EasyPeasy.App.Components.Pages.Drilling.Cards.ReviewStudyCardCard;

namespace EasyPeasy.App.Components.Pages.Drilling.Definitions;

// ── Review ────────────────────────────────────────────────────────────────────

public sealed class ReviewStudyCardsDef : TestDefinition<StudyCardTestModel>
{
    public override string Key         => "review-study-cards";
    public override string Title       => "Перегляд карток";
    public override string HeaderClass => "pastel-mint";
    public override string IconClass   => "bi-journal-text";

    // RawItem card — BuildViewModel is not needed
    public override bool           IsBrowse                    => true;
    public override bool           ShowNextButton(TestState s) => true;
    public override NextItemAction GetNextAction(TestState s)  => NextItemAction.Remove;
    public override Type           ComponentType               => typeof(ReviewStudyCard);

    public override void    RecordRating(StudyCardTestModel item, double rating) => item.Rate = (float)rating;
    public override double? GetRating(StudyCardTestModel item)                   => item.Rate;

    public override string GetLabel(StudyCardTestModel item) =>
        !string.IsNullOrWhiteSpace(item.Title) ? item.Title : (item.Body ?? "");
    public override void    OnItemCompleted(StudyCardTestModel item)             => item.RecordReview();
}
