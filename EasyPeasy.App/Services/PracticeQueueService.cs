namespace EasyPeasy.App.Services;

/// <summary>
/// Holds an ordered queue of practice segments for a course (each segment is a single-content-type
/// session: words, examples, irregular forms, study cards, or test cards).
/// State is in-memory only, for the duration of one course-practice run.
/// </summary>
public class PracticeQueueService
{
    private List<string> _segmentRoutes = new();
    private int _currentIndex = -1;
    private string? _finalRoute;

    /// <summary>Whether a queue is currently running (started via <see cref="Start"/> and not yet exhausted/reset).</summary>
    public bool IsActive => _currentIndex >= 0 && _currentIndex < _segmentRoutes.Count;

    /// <summary>Number of segments in the current queue.</summary>
    public int TotalSegments => _segmentRoutes.Count;

    /// <summary>1-based index of the current segment, for progress display (e.g. "3 of 7").</summary>
    public int CurrentPosition => _currentIndex + 1;

    /// <summary>Starts a new queue and returns the route of the first segment.</summary>
    public string Start(IEnumerable<string> segmentRoutes, string finalRoute)
    {
        _segmentRoutes = segmentRoutes.ToList();
        _finalRoute = finalRoute;
        _currentIndex = 0;

        if (_segmentRoutes.Count == 0)
        {
            Reset();
            return finalRoute;
        }

        return _segmentRoutes[0];
    }

    /// <summary>
    /// Advances the queue to the next segment. Returns the next segment's route, or the final
    /// route (and resets the queue) once there are no segments left. Returns null if the queue
    /// isn't active (a standalone practice session outside a course run).
    /// </summary>
    public string? Advance()
    {
        if (!IsActive)
            return null;

        _currentIndex++;

        if (_currentIndex < _segmentRoutes.Count)
            return _segmentRoutes[_currentIndex];

        var finalRoute = _finalRoute;
        Reset();
        return finalRoute;
    }

    /// <summary>Clears the queue back to "inactive" — <see cref="IsActive"/> becomes <c>false</c>.</summary>
    public void Reset()
    {
        _segmentRoutes = new();
        _currentIndex = -1;
        _finalRoute = null;
    }
}
