using EasyEnglish.App.Models;
using EasyEnglish.App.Components.Pages.Drilling.Models;
using ReviewStudyCard = EasyEnglish.App.Components.Pages.Drilling.Cards.ReviewStudyCardCard;

namespace EasyEnglish.App.Components.Pages.Drilling.Definitions;

// ── Review ────────────────────────────────────────────────────────────────────

public sealed class ReviewStudyCardsDef : TestDefinition<StudyCardTestModel>
{
    public override string Key         => "review-study-cards";
    public override string Title       => "Перегляд карток";
    public override string HeaderClass => "pastel-mint";
    public override string IconClass   => "bi-journal-text";

    // RawItem-картка — BuildViewModel не потрібен
    public override bool           ShowNextButton(TestState s) => true;
    public override NextItemAction GetNextAction(TestState s)  => NextItemAction.Remove;
    public override Type           ComponentType               => typeof(ReviewStudyCard);

    public override void RecordRating(StudyCardTestModel item, double rating) => item.Rate = (float)rating;
    public override void OnItemCompleted(StudyCardTestModel item)             => item.RecordReview();
}
