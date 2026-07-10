using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Interfaces.Fields;
using EasyEnglish.Core.Options;
using Microsoft.EntityFrameworkCore;

namespace EasyEnglish.Data.Extensions;

/// <summary>
/// Shared item-selection logic for learning (words, irregular forms, cards).
/// Applies to any entity that carries a rating and review-tracking info.
/// </summary>
/// <remarks>
/// Rate/LastReviewDate/CreatedAt are read via <see cref="EF.Property{TProperty}"/> instead of direct
/// interface member access — accessing an interface member on a generic T doesn't always get translated
/// to SQL by the EF Core provider, whereas EF.Property is guaranteed to translate.
/// </remarks>
public static class LearningQueryExtensions
{
    /// <summary>
    /// Applies filtering/ordering/count-limiting per <paramref name="options"/> and materializes the result.
    /// Random priority can't be expressed as SQL ordering followed by Take — that would just return a
    /// fixed-order slice — so it's the one case that needs the full filtered set pulled into memory first.
    /// </summary>
    public static async Task<List<T>> ApplyLearningSelectionAsync<T>(this IQueryable<T> query, LearningSelectionOptions options)
        where T : class, IRateInfo, IReviewInfo
    {
        if (!options.IncludeLearnedWords)
            query = query.Where(w => EF.Property<float>(w, nameof(IRateInfo.Rate)) >= 1.6f);

        if (options.Priority == LearningPriority.Random)
        {
            var all = await query.ToListAsync();
            var shuffled = all.OrderBy(_ => Random.Shared.Next()).ToList();
            return options.WordCount > 0 ? shuffled.Take(options.WordCount).ToList() : shuffled;
        }

        query = options.Priority switch
        {
            LearningPriority.New => query
                .Where(w => EF.Property<DateTime?>(w, nameof(IReviewInfo.LastReviewDate)) == null)
                .OrderByDescending(w => EF.Property<DateTime>(w, "CreatedAt")),

            LearningPriority.Difficult => query
                .OrderByDescending(w => EF.Property<float>(w, nameof(IRateInfo.Rate))),

            LearningPriority.Review => query
                .Where(w => EF.Property<DateTime?>(w, nameof(IReviewInfo.LastReviewDate)) != null)
                .OrderBy(w => EF.Property<DateTime?>(w, nameof(IReviewInfo.LastReviewDate))),

            LearningPriority.Old => query
                .Where(w => EF.Property<DateTime?>(w, nameof(IReviewInfo.LastReviewDate)) == null)
                .OrderBy(w => EF.Property<DateTime>(w, "CreatedAt")),

            _ => query
        };

        if (options.WordCount > 0)
            query = query.Take(options.WordCount);

        return await query.ToListAsync();
    }
}
