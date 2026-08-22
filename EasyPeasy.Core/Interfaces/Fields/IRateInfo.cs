namespace EasyPeasy.Core.Interfaces.Fields;

/// <summary>
/// Implemented by entities/models with a learner-facing difficulty rating on the <c>[0, 5]</c> scale.
/// See <see cref="EasyPeasy.Core.Extensions.RateExtensions"/> for bucketing into <see cref="EasyPeasy.Core.Enums.DifficultyLevel"/>.
/// </summary>
public interface IRateInfo
{
    float Rate { get; set; }
}
