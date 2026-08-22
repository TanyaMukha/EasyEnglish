using EasyPeasy.App.Services;

namespace EasyPeasy.App.Tests;

/// <summary>
/// Tests for <see cref="ConfirmationService"/> — the gate every deletion in the app goes through.
/// Covered here because the failure modes are silent: a task that completes on its own would delete
/// without asking, and one that never completes would leave the page frozen mid-delete.
/// </summary>
public class ConfirmationServiceTests
{
    private static ConfirmRequest AnyRequest() =>
        new() { Title = "Видалити?", Message = "Видалити слово «cat»?" };

    [Fact]
    public void ConfirmAsync_KeepsWaitingUntilTheUserAnswers()
    {
        var service = new ConfirmationService();

        var pending = service.ConfirmAsync(AnyRequest());

        Assert.False(pending.IsCompleted);
        Assert.NotNull(service.Current);
    }

    [Fact]
    public async Task Respond_True_CompletesWithTrueAndClosesTheDialog()
    {
        var service = new ConfirmationService();
        var pending = service.ConfirmAsync(AnyRequest());

        service.Respond(true);

        Assert.True(await pending);
        Assert.Null(service.Current);
    }

    [Fact]
    public async Task Respond_False_CompletesWithFalse()
    {
        var service = new ConfirmationService();
        var pending = service.ConfirmAsync(AnyRequest());

        service.Respond(false);

        Assert.False(await pending);
        Assert.Null(service.Current);
    }

    [Fact]
    public async Task ConfirmAsync_SecondRequest_AnswersTheFirstOneWithNo()
    {
        var service = new ConfirmationService();

        var first  = service.ConfirmAsync(AnyRequest());
        var second = service.ConfirmAsync(AnyRequest());

        // The first caller must stop rather than delete behind a dialog it lost.
        Assert.False(await first);
        Assert.False(second.IsCompleted);
    }

    [Fact]
    public void Respond_WithNoOpenDialog_DoesNothing()
    {
        var service = new ConfirmationService();

        service.Respond(true);

        Assert.Null(service.Current);
    }

    [Fact]
    public void OnChange_FiresWhenTheDialogOpensAndWhenItCloses()
    {
        var service = new ConfirmationService();
        var changes = 0;
        service.OnChange += () => changes++;

        service.ConfirmAsync(AnyRequest());
        service.Respond(true);

        Assert.Equal(2, changes);
    }

    [Fact]
    public void ConfirmDeleteAsync_WithName_QuotesItAndAppendsTheConsequences()
    {
        var service = new ConfirmationService();

        service.ConfirmDeleteAsync("слово", "cat", "Приклади цього слова зникнуть разом із ним.");

        Assert.Equal(
            "Видалити слово «cat»? Цю дію не можна скасувати. Приклади цього слова зникнуть разом із ним.",
            service.Current!.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ConfirmDeleteAsync_WithoutName_AsksAboutTheKindAlone(string? name)
    {
        var service = new ConfirmationService();

        service.ConfirmDeleteAsync("тестову картку", name);

        Assert.Equal("Видалити тестову картку? Цю дію не можна скасувати.", service.Current!.Message);
    }

    [Fact]
    public void ConfirmDeleteAsync_DefaultsToADestructiveDialog()
    {
        var service = new ConfirmationService();

        service.ConfirmDeleteAsync("курс", "Business English");

        Assert.True(service.Current!.IsDestructive);
        Assert.Equal("Видалити", service.Current.ConfirmText);
        Assert.Equal("Скасувати", service.Current.CancelText);
    }
}
