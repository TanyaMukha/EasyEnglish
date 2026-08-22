using EasyPeasy.App.Components.Pages.Drilling.Models;

namespace EasyPeasy.App.Tests;

/// <summary>
/// Tests for <see cref="MultiTestEngine{TItem}"/> — the drilling session rules that used to live
/// inside MultiTestHost.razor: two phases, queue handling, manual ratings and review bookkeeping.
/// Covered here because these are the parts where a wrong decision silently corrupts a learner's
/// progress (a review recorded for a card never opened, a rating that cannot be undone,
/// an item dropped from the queue after a wrong answer).
/// </summary>
public class MultiTestEngineTests
{
    // ── Test doubles ──────────────────────────────────────────────────────────

    private sealed class Card
    {
        public string Word    { get; init; } = "";
        public double Rate    { get; set; }
        public int    Reviews { get; set; }
        public int    Correct { get; set; }
        public int    Wrong   { get; set; }
    }

    /// <summary>Browse definition: flipped through, rates cards, records a review on completion.</summary>
    private sealed class BrowseDef : TestDefinition<Card>
    {
        public override string Key         => "browse";
        public override string Title       => "Перегляд";
        public override string HeaderClass => "";
        public override string IconClass   => "";
        public override Type   ComponentType => typeof(object);

        public override bool           IsBrowse                    => true;
        public override bool           ShowNextButton(TestState s) => true;
        public override NextItemAction GetNextAction(TestState s)  => NextItemAction.Remove;

        public override WordCardViewModel BuildViewModel(Card item) => new(item.Word, null, null);
        public override string  GetLabel(Card item)                 => item.Word;
        public override double? GetRating(Card item)                => item.Rate;
        public override void    RecordRating(Card item, double r)   => item.Rate = r;
        public override void    OnItemCompleted(Card item)          => item.Reviews++;
    }

    /// <summary>Test definition: answered, removed when correct, requeued when wrong.</summary>
    private class QuizDef : TestDefinition<Card>
    {
        public override string Key         => "quiz";
        public override string Title       => "Тест";
        public override string HeaderClass => "";
        public override string IconClass   => "";
        public override Type   ComponentType => typeof(object);

        public override bool           ShowNextButton(TestState s) => s.IsAnswerSubmitted;
        public override NextItemAction GetNextAction(TestState s)  =>
            s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;

        public override WordCardViewModel BuildViewModel(Card item) => new(item.Word, null, null);

        public override void RecordAnswer(Card item, bool isCorrect)
        {
            if (isCorrect) item.Correct++;
            else           item.Wrong++;
        }
    }

    /// <summary>Definition that applies to nothing — used to check filtering.</summary>
    private sealed class NeverAppliesDef : QuizDef
    {
        public override bool CanApplyTo(Card item) => false;
    }

    private static List<Card> Cards(params string[] words) =>
        words.Select(w => new Card { Word = w, Rate = 3 }).ToList();

    private static MultiTestEngine<Card> Engine(
        IReadOnlyList<TestDefinition<Card>> defs,
        IReadOnlyList<Card>                 items)
        => new(defs, items, new Random(1));   // fixed seed: shuffling stays reproducible

    // ── Phases ────────────────────────────────────────────────────────────────

    [Fact]
    public void Starts_in_browse_phase_when_a_browse_definition_is_selected()
    {
        var items  = Cards("one", "two");
        var engine = Engine([new BrowseDef(), new QuizDef()], items);

        Assert.True(engine.InBrowsePhase);
        Assert.Equal("one", engine.CurrentItem!.Word);   // browse keeps source order
        Assert.True(engine.HasTestsAhead);
    }

    [Fact]
    public void Starts_in_test_phase_when_no_browse_definition_is_selected()
    {
        var engine = Engine([new QuizDef()], Cards("one", "two"));

        Assert.False(engine.InBrowsePhase);
        Assert.False(engine.ShowStats);
        Assert.Equal(2, engine.TotalItems);
    }

    [Fact]
    public void Shows_stats_immediately_when_nothing_matches_any_definition()
    {
        var engine = Engine([new NeverAppliesDef()], Cards("one"));

        Assert.True(engine.ShowStats);
        Assert.Equal(0, engine.TotalItems);
    }

    // ── Browse navigation ─────────────────────────────────────────────────────

    [Fact]
    public void Browse_wraps_around_in_both_directions()
    {
        var engine = Engine([new BrowseDef()], Cards("one", "two", "three"));

        engine.Next();
        Assert.Equal("two", engine.CurrentItem!.Word);

        engine.Next();
        engine.Next();                                   // past the last one comes the first again
        Assert.Equal("one", engine.CurrentItem!.Word);

        engine.Previous();                               // from the first one back — to the end
        Assert.Equal("three", engine.CurrentItem!.Word);
    }

    [Fact]
    public void Browse_never_shrinks_the_card_list()
    {
        var engine = Engine([new BrowseDef()], Cards("one", "two"));

        engine.Next();
        engine.Next();
        engine.Next();

        Assert.Equal(2, engine.BrowseCards.Count);
        Assert.True(engine.InBrowsePhase);
        Assert.False(engine.ShowStats);
    }

    [Fact]
    public void Jump_moves_to_the_requested_card_and_ignores_out_of_range()
    {
        var engine = Engine([new BrowseDef()], Cards("one", "two", "three"));

        engine.JumpTo(2);
        Assert.Equal("three", engine.CurrentItem!.Word);

        engine.JumpTo(99);
        Assert.Equal("three", engine.CurrentItem!.Word);
    }

    // ── Review bookkeeping ────────────────────────────────────────────────────

    [Fact]
    public void Leaving_browse_records_a_review_only_for_opened_cards()
    {
        var items  = Cards("one", "two", "three");
        var engine = Engine([new BrowseDef(), new QuizDef()], items);

        engine.Next();          // "one" and "two" were opened, "three" was not
        engine.GoToTests();

        Assert.Equal(1, items[0].Reviews);
        Assert.Equal(1, items[1].Reviews);
        Assert.Equal(0, items[2].Reviews);
    }

    [Fact]
    public void Flipping_back_and_forth_records_one_review_per_card()
    {
        var items  = Cards("one", "two");
        var engine = Engine([new BrowseDef()], items);

        engine.Next();
        engine.Previous();
        engine.Next();
        engine.FinishEarly();

        Assert.Equal(1, items[0].Reviews);
        Assert.Equal(1, items[1].Reviews);
    }

    [Fact]
    public void Go_to_tests_switches_phase_and_keeps_the_test_queue()
    {
        var engine = Engine([new BrowseDef(), new QuizDef()], Cards("one", "two"));

        engine.GoToTests();

        Assert.False(engine.InBrowsePhase);
        Assert.False(engine.ShowStats);
        Assert.Equal(2, engine.TotalItems);
    }

    [Fact]
    public void Go_to_tests_finishes_the_session_when_only_browsing_was_selected()
    {
        var engine = Engine([new BrowseDef()], Cards("one", "two"));

        engine.GoToTests();

        Assert.True(engine.ShowStats);
        Assert.Equal(2, engine.DisplayTotal);   // the summary counts browsed cards, not zero
    }

    [Fact]
    public void Finish_early_ends_the_session_from_the_test_phase()
    {
        var engine = Engine([new QuizDef()], Cards("one", "two"));

        engine.RecordAnswer(isCorrect: true);
        engine.FinishEarly();

        Assert.True(engine.ShowStats);
        Assert.Equal(1, engine.CorrectAnswers);
    }

    // ── Ratings ───────────────────────────────────────────────────────────────

    [Fact]
    public void Rating_a_card_writes_it_through_to_the_item()
    {
        var items  = Cards("one");
        var engine = Engine([new BrowseDef()], items);

        engine.Rate(5);

        Assert.True(engine.State.IsRated);
        Assert.Equal(5, items[0].Rate);
    }

    [Fact]
    public void Picking_the_same_level_again_clears_it_and_restores_the_original_rating()
    {
        var items  = Cards("one");
        items[0].Rate = 2.5;
        var engine = Engine([new BrowseDef()], items);

        engine.Rate(5);
        engine.Rate(5);

        Assert.False(engine.State.IsRated);
        Assert.Equal(2.5, items[0].Rate);
    }

    [Fact]
    public void Picking_another_level_replaces_the_rating_without_restoring()
    {
        var items  = Cards("one");
        var engine = Engine([new BrowseDef()], items);

        engine.Rate(5);
        engine.Rate(1);

        Assert.True(engine.State.IsRated);
        Assert.Equal(1, engine.State.LastRatedValue);
        Assert.Equal(1, items[0].Rate);
    }

    [Fact]
    public void Rating_survives_flipping_away_and_back()
    {
        var engine = Engine([new BrowseDef()], Cards("one", "two"));

        engine.Rate(4);
        engine.Next();
        Assert.False(engine.State.IsRated);      // the neighbouring card has no rating

        engine.Previous();
        Assert.True(engine.State.IsRated);
        Assert.Equal(4, engine.State.LastRatedValue);
    }

    [Fact]
    public void Clearing_after_a_round_trip_still_restores_the_original_rating()
    {
        var items  = Cards("one", "two");
        items[0].Rate = 3;
        var engine = Engine([new BrowseDef()], items);

        engine.Rate(5);
        engine.Next();
        engine.Previous();
        engine.Rate(5);                          // clear the pick after coming back

        Assert.False(engine.State.IsRated);
        Assert.Equal(3, items[0].Rate);
    }

    [Fact]
    public void Card_list_shows_current_ratings_and_visited_marks()
    {
        var engine = Engine([new BrowseDef()], Cards("one", "two"));

        engine.Rate(5);

        var cards = engine.BrowseCards;
        Assert.Equal(["one", "two"], cards.Select(c => c.Label));
        Assert.Equal(5, cards[0].Rating);
        Assert.True(cards[0].IsVisited);
        Assert.False(cards[1].IsVisited);
    }

    // ── Test queue ────────────────────────────────────────────────────────────

    [Fact]
    public void Correct_answer_removes_the_item_from_the_queue()
    {
        var items  = Cards("one", "two");
        var engine = Engine([new QuizDef()], items);

        engine.RecordAnswer(isCorrect: true);
        engine.Next();

        Assert.Equal(1, engine.CompletedCount);
        Assert.Equal(1, engine.CorrectAnswers);
        Assert.False(engine.ShowStats);
    }

    [Fact]
    public void Wrong_answer_keeps_the_item_in_the_queue()
    {
        var engine = Engine([new QuizDef()], Cards("one", "two"));

        engine.RecordAnswer(isCorrect: false);
        engine.Next();

        Assert.Equal(0, engine.CompletedCount);
        Assert.Equal(1, engine.IncorrectAnswers);
        Assert.False(engine.ShowStats);
    }

    [Fact]
    public void Session_ends_when_the_queue_empties()
    {
        var engine = Engine([new QuizDef()], Cards("one"));

        engine.RecordAnswer(isCorrect: true);
        engine.Next();

        Assert.True(engine.ShowStats);
        Assert.Equal(1, engine.CompletedCount);
    }

    [Fact]
    public void A_requeued_item_can_still_be_completed_later()
    {
        var items  = Cards("one");
        var engine = Engine([new QuizDef()], items);

        engine.RecordAnswer(isCorrect: false);
        engine.Next();
        engine.RecordAnswer(isCorrect: true);
        engine.Next();

        Assert.True(engine.ShowStats);
        Assert.Equal(1, items[0].Correct);
        Assert.Equal(1, items[0].Wrong);
        Assert.Equal(2, engine.TotalAttempts);
    }

    [Fact]
    public void Next_button_follows_the_definition_in_the_test_phase()
    {
        var engine = Engine([new QuizDef()], Cards("one"));

        Assert.False(engine.ShowNextButton);     // nothing answered yet

        engine.RecordAnswer(isCorrect: true);
        Assert.True(engine.ShowNextButton);
    }

    [Fact]
    public void Reveal_counts_as_an_attempt()
    {
        var engine = Engine([new QuizDef()], Cards("one"));

        engine.Reveal();

        Assert.True(engine.State.IsRevealed);
        Assert.Equal(1, engine.TotalAttempts);
    }
}
