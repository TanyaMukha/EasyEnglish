namespace EasyPeasy.App.Components.Pages.Drilling.Models;

/// <summary>
/// DTO що передається в кожен card-компонент через DynamicComponent.
/// </summary>
public class TestCardContext
{
    /// <summary>
    /// ViewModel для карток типу "слово" (word, irregular forms).
    /// Будується через TestDefinition.BuildViewModel().
    /// </summary>
    public required WordCardViewModel ViewModel { get; init; }

    /// <summary>
    /// Сирий item (boxing) — використовується картками прикладів,
    /// яким потрібен ExampleModel безпосередньо (Sentence, markdown тощо).
    /// Картки типу "слово" ігнорують це поле.
    /// </summary>
    public object? RawItem { get; init; }

    public required TestState     State     { get; init; }
    public required TestCallbacks Callbacks { get; init; }

    /// <summary>Хелпер для карток прикладів: повертає RawItem як TItem.</summary>
    public T GetRawItem<T>() where T : class =>
        RawItem as T ?? throw new InvalidOperationException(
            $"RawItem is not {typeof(T).Name}. Got: {RawItem?.GetType().Name ?? "null"}");
}
