using EasyPeasy.Core.Enums;
using EasyPeasy.Core.Extensions;
using EasyPeasy.Core.Interfaces.Fields;
using EasyPeasy.Core.Options;
using Microsoft.EntityFrameworkCore;

namespace EasyPeasy.Data.Extensions;

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
            query = query.Where(w => EF.Property<float>(w, nameof(IRateInfo.Rate)) >= RateExtensions.EasyMax);

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

            // Spans every item, not just never-reviewed ones (unlike New): ranks by whichever
            // timestamp reflects "last touched" -- LastReviewDate if it's been reviewed, CreatedAt
            // otherwise -- so a long-neglected never-reviewed item and a long-ago-reviewed item both
            // surface as "old", ordered oldest-touched first.
            LearningPriority.Old => query
                .OrderBy(w => EF.Property<DateTime?>(w, nameof(IReviewInfo.LastReviewDate))
                    ?? EF.Property<DateTime>(w, "CreatedAt")),

            // "What I studied before this": reviewed items, newest review first. Never-reviewed
            // items are excluded rather than sorted last — they were never "studied before".
            LearningPriority.Recent => query
                .Where(w => EF.Property<DateTime?>(w, nameof(IReviewInfo.LastReviewDate)) != null)
                .OrderByDescending(w => EF.Property<DateTime?>(w, nameof(IReviewInfo.LastReviewDate))),

            _ => query
        };

        if (options.WordCount > 0)
            query = query.Take(options.WordCount);

        return await query.ToListAsync();
    }
}
