namespace EasyPeasy.App.Models;

/// <summary>Describes a single exercise type shown in the exercise picker (ExerciseTypeList).</summary>
public class ExerciseOption
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-card-text";
    public string Color { get; set; } = "blue";

    /// <summary>
    /// The exercise listens to the learner's voice. Where speech recognition is unavailable
    /// (Android today), the picker hides it instead of offering an exercise that cannot work.
    /// </summary>
    public bool RequiresSpeech { get; set; }

    /// <summary>Whether the exercise is part of the default selection ("preset" in the picker).</summary>
    public bool InDefaultPreset { get; set; } = true;
}
