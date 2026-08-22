namespace EasyPeasy.App.Components.Pages.Drilling.Models;

/// <summary>
/// State and rules of one multi-test session: two phases, the queue, ratings, statistics.
/// There is deliberately no Blazor here — MultiTestHost only renders what the engine decided,
/// so the logic can be covered by ordinary unit tests.
///
/// Browse phase (definitions with <see cref="TestDefinition{TItem}.IsBrowse"/>) is a looping card
/// list: flip back and forth, nothing disappears, leaving is explicit (<see cref="GoToTests"/>
/// or <see cref="FinishEarly"/>). Test phase is a queue: a correct answer removes the card,
/// a wrong one puts it back a few positions later.
/// </summary>
public sealed class MultiTestEngine<TItem>
{
    private readonly IReadOnlyList<TItem> _items;
    private readonly Random               _rng;

    private readonly List<(TItem item, TestDefinition<TItem> def)> _browse;
    private readonly List<(TItem item, TestDefinition<TItem> def)> _pending;

    /// <summary>Browse indices the learner actually opened — only those get a review recorded.</summary>
    private readonly HashSet<int> _browseVisited = [];

    /// <summary>Manual rating per browse index, so it survives flipping away and back.</summary>
    private readonly Dictionary<int, double> _browseRatings = [];

    /// <summary>Rating the card had before the manual pick — restored when the pick is cleared.</summary>
    private readonly Dictionary<object, double?> _originalRatings = new(ReferenceEqualityComparer.Instance);

    private List<BrowseCardInfo> _browseCards = [];
    private int _currentIndex;

    public MultiTestEngine(
        IReadOnlyList<TestDefinition<TItem>> definitions,
        IReadOnlyList<TItem>                 items,
        Random?                              rng = null)
    {
        _items = items;
        _rng   = rng ?? new Random();

        // Browsing keeps source order — flipping back and forth through a shuffled list is confusing.
        _browse = definitions
            .Where(def => def.IsBrowse)
            .SelectMany(def => items
                .Where(def.CanApplyTo)
                .Select(item => (item, def)))
            .ToList();

        _pending = definitions
            .Where(def => !def.IsBrowse)
            .SelectMany(def => items
                .Where(def.CanApplyTo)
                .OrderBy(_ => _rng.NextDouble())   // items shuffled within a type
                .Select(item => (item, def)))
            .ToList();                              // types stay in the definitions order

        TotalItems = _pending.Count;

        if (_browse.Count > 0)
        {
            InBrowsePhase = true;
            LoadBrowse();
        }
        else if (_pending.Count > 0)
        {
            LoadCurrent();
        }
        else
        {
            ShowStats = true;
        }
    }

    // ── State ─────────────────────────────────────────────────────────────────

    public TestState State { get; } = new();

    public bool InBrowsePhase { get; private set; }
    public bool ShowStats     { get; private set; }
    public int  BrowseIndex   { get; private set; }

    public TItem?                 CurrentItem       { get; private set; }
    public TestDefinition<TItem>? CurrentDefinition { get; private set; }
    public WordCardViewModel?     CurrentViewModel  { get; private set; }

    public IReadOnlyList<BrowseCardInfo> BrowseCards => _browseCards;

    public int TotalItems       { get; }
    public int CorrectAnswers   { get; private set; }
    public int IncorrectAnswers { get; private set; }
    public int TotalAttempts    { get; private set; }

    public int  CompletedCount => TotalItems - _pending.Count;
    public bool HasTestsAhead  => _pending.Count > 0;

    /// <summary>"Next" is always available while browsing; in tests the definition decides.</summary>
    public bool ShowNextButton => InBrowsePhase || (CurrentDefinition?.ShowNextButton(State) ?? false);

    /// <summary>Progress-bar position: card number while browsing, completed count in tests.</summary>
    public int ProgressCompleted => InBrowsePhase ? BrowseIndex : CompletedCount;

    /// <summary>
    /// How many cards the current phase has. If no tests were selected at all (browsing only),
    /// the summary shows how many cards were browsed instead of zero.
    /// </summary>
    public int DisplayTotal => InBrowsePhase
        ? _browse.Count
        : (TotalItems > 0 ? TotalItems : _browse.Count);

    /// <summary>Rating of the card on screen, if this definition tracks a rating at all.</summary>
    public double? CurrentRating =>
        CurrentItem is not null ? CurrentDefinition?.GetRating(CurrentItem) : null;

    // ── Loading a card ────────────────────────────────────────────────────────

    private void LoadBrowse()
    {
        if (_browse.Count == 0) return;

        var (item, def) = _browse[BrowseIndex];
        LoadEntry(item, def);

        // Restore the rating the learner already picked for this card in this session.
        if (_browseRatings.TryGetValue(BrowseIndex, out var rated))
        {
            State.IsRated        = true;
            State.LastRatedValue = rated;
        }

        _browseVisited.Add(BrowseIndex);
        RebuildBrowseCards();
    }

    private void LoadCurrent()
    {
        if (_pending.Count == 0) return;

        var (item, def) = _pending[_currentIndex];
        LoadEntry(item, def);
    }

    private void LoadEntry(TItem item, TestDefinition<TItem> def)
    {
        CurrentItem       = item;
        CurrentDefinition = def;

        State.Reset();

        CurrentViewModel = def.BuildViewModel(item);

        State.CorrectAnswer  = def.GetCorrectAnswer(CurrentViewModel);
        State.WordIsQuestion = def.WordIsQuestion;

        def.PrepareState(CurrentViewModel, _items, def.BuildViewModel, State);

        if (item is not null && !_originalRatings.ContainsKey(item))
            _originalRatings[item] = def.GetRating(item);
    }

    private void RebuildBrowseCards() =>
        _browseCards = _browse
            .Select((entry, index) => new BrowseCardInfo(
                Index:     index,
                Label:     entry.def.GetLabel(entry.item),
                Rating:    entry.def.GetRating(entry.item),
                IsVisited: _browseVisited.Contains(index)))
            .ToList();

    // ── Answers ───────────────────────────────────────────────────────────────

    public void RecordAnswer(bool isCorrect)
    {
        State.IsAnswerSubmitted = true;
        State.IsCorrect         = isCorrect;
        TotalAttempts++;

        if (isCorrect) CorrectAnswers++;
        else           IncorrectAnswers++;

        if (CurrentItem is not null)
            CurrentDefinition?.RecordAnswer(CurrentItem, isCorrect);
    }

    public void Reveal()
    {
        State.IsRevealed = true;
        TotalAttempts++;
    }

    /// <summary>
    /// Sets the difficulty rating. Picking the same level again clears it and restores the
    /// rating the card had before the manual pick.
    /// </summary>
    public void Rate(double rating)
    {
        var isSameLevel = State.IsRated && Math.Abs(State.LastRatedValue - rating) < 0.001;

        if (isSameLevel)
        {
            State.IsRated        = false;
            State.LastRatedValue = 0;

            if (InBrowsePhase)
                _browseRatings.Remove(BrowseIndex);

            if (CurrentItem is not null
                && _originalRatings.TryGetValue(CurrentItem, out var original)
                && original is not null)
            {
                CurrentDefinition?.RecordRating(CurrentItem, original.Value);
            }
        }
        else
        {
            State.IsRated        = true;
            State.LastRatedValue = rating;

            if (InBrowsePhase)
                _browseRatings[BrowseIndex] = rating;

            if (CurrentItem is not null)
                CurrentDefinition?.RecordRating(CurrentItem, rating);
        }

        if (InBrowsePhase)
            RebuildBrowseCards();
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    public void Next()
    {
        if (InBrowsePhase)
        {
            BrowseIndex = (BrowseIndex + 1) % _browse.Count;
            LoadBrowse();
            return;
        }

        if (CurrentDefinition is null) return;

        if (CurrentDefinition.GetNextAction(State) == NextItemAction.Remove)
        {
            if (CurrentItem is not null)
                CurrentDefinition.OnItemCompleted(CurrentItem);

            _pending.RemoveAt(_currentIndex);
        }
        else
        {
            var entry    = _pending[_currentIndex];
            var offset   = _rng.Next(2, 5);
            var insertAt = Math.Min(_currentIndex + offset, _pending.Count - 1);
            _pending.RemoveAt(_currentIndex);
            _pending.Insert(insertAt, entry);
        }

        if (_pending.Count > 0)
        {
            _currentIndex %= _pending.Count;
            LoadCurrent();
        }
        else
        {
            ShowStats = true;
        }
    }

    /// <summary>Previous browse card — the list wraps around.</summary>
    public void Previous()
    {
        if (!InBrowsePhase || _browse.Count == 0) return;

        BrowseIndex = (BrowseIndex - 1 + _browse.Count) % _browse.Count;
        LoadBrowse();
    }

    /// <summary>Jump to an arbitrary card from the browse list.</summary>
    public void JumpTo(int index)
    {
        if (!InBrowsePhase || index < 0 || index >= _browse.Count) return;

        BrowseIndex = index;
        LoadBrowse();
    }

    /// <summary>Ends browsing and moves on to the tests (or to the summary when there are none).</summary>
    public void GoToTests()
    {
        CompleteBrowsePhase();

        if (_pending.Count > 0) LoadCurrent();
        else                    ShowStats = true;
    }

    /// <summary>Early finish — show the results of whatever was done so far.</summary>
    public void FinishEarly()
    {
        CompleteBrowsePhase();
        ShowStats = true;
    }

    /// <summary>Records a review for every opened card and leaves the browse phase.</summary>
    private void CompleteBrowsePhase()
    {
        if (!InBrowsePhase) return;

        foreach (var index in _browseVisited)
        {
            var (item, def) = _browse[index];
            if (item is not null)
                def.OnItemCompleted(item);
        }

        InBrowsePhase = false;
    }
}
