namespace EasyEnglish.App.Models;

/// <summary>Describes a single exercise type shown in the exercise-picker grid (ExerciseTypeGrid).</summary>
public class ExerciseOption
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-card-text";
    public string Color { get; set; } = "blue";
}
