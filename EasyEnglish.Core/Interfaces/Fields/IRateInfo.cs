namespace EasyEnglish.Core.Interfaces.Fields;

/// <summary>
/// Implemented by entities/models with a learner-facing difficulty rating on the <c>[0, 5]</c> scale.
/// See <see cref="EasyEnglish.Core.Extensions.RateExtensions"/> for bucketing into <see cref="EasyEnglish.Core.Enums.DifficultyLevel"/>.
/// </summary>
public interface IRateInfo
{
    float Rate { get; set; }
}
