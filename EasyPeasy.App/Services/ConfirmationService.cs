namespace EasyPeasy.App.Services;

/// <summary>
/// One confirmation the user has to answer before a destructive action runs.
/// </summary>
public sealed record ConfirmRequest
{
    /// <summary>Dialog heading — names the action about to happen.</summary>
    public required string Title { get; init; }

    /// <summary>What exactly disappears, named so a wrong target is easy to spot.</summary>
    public required string Message { get; init; }

    public string ConfirmText { get; init; } = "Видалити";

    public string CancelText { get; init; } = "Скасувати";

    /// <summary>Paints the confirm button red. Turn off for confirmations that destroy nothing.</summary>
    public bool IsDestructive { get; init; } = true;
}

/// <summary>
/// The gate every deletion passes through: a page awaits <see cref="ConfirmAsync"/> and touches the
/// database only when it returns <c>true</c>. <c>ConfirmDialogHost</c>, rendered once in MainLayout,
/// draws the dialog and reports the answer back, so the wording and the styling stay the same
/// everywhere and no page can delete without an explicit "yes".
/// </summary>
/// <remarks>
/// Scoped per BlazorWebView, so the app has at most one open dialog. A request arriving while another
/// is still open answers the older one with <c>false</c> — its caller stops instead of deleting behind
/// a dialog the user never got to read.
/// </remarks>
public sealed class ConfirmationService
{
    private TaskCompletionSource<bool>? _pending;

    /// <summary>The request currently on screen, or <c>null</c> when no dialog is open.</summary>
    public ConfirmRequest? Current { get; private set; }

    /// <summary>Raised whenever the dialog has to appear or disappear.</summary>
    public event Action? OnChange;

    /// <summary>Shows the dialog and completes with the user's answer.</summary>
    public Task<bool> ConfirmAsync(ConfirmRequest request)
    {
        _pending?.TrySetResult(false);

        Current  = request;
        _pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        OnChange?.Invoke();
        return _pending.Task;
    }

    /// <summary>
    /// Deletion of one named record — the wording shared by every list and details page.
    /// </summary>
    /// <param name="what">What is being deleted, in the accusative case: "курс", "слово".</param>
    /// <param name="name">The record's own title, quoted in the message when it has one.</param>
    /// <param name="consequences">What else goes with it — cascaded children, broken links.</param>
    public Task<bool> ConfirmDeleteAsync(string what, string? name = null, string? consequences = null)
    {
        var target  = string.IsNullOrWhiteSpace(name) ? what : $"{what} «{name.Trim()}»";
        var message = $"Видалити {target}? Цю дію не можна скасувати.";

        if (!string.IsNullOrWhiteSpace(consequences))
            message = $"{message} {consequences.Trim()}";

        return ConfirmAsync(new ConfirmRequest
        {
            Title   = "Підтвердження видалення",
            Message = message,
        });
    }

    /// <summary>Answers the open dialog. A no-op when nothing is waiting for an answer.</summary>
    public void Respond(bool confirmed)
    {
        var pending = _pending;
        if (pending is null)
            return;

        _pending = null;
        Current  = null;

        OnChange?.Invoke();
        pending.TrySetResult(confirmed);
    }
}
