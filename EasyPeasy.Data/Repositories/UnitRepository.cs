using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;
using EasyPeasy.Core.Models;
using EasyPeasy.Core.Extensions;

namespace EasyPeasy.Data.Repositories;

/// <summary>EF Core-backed <see cref="IUnitRepository"/>.</summary>
public class UnitRepository : BaseWithGuidRepository<UnitEntity, EasyPeasyDbContext>, IUnitRepository
{
    public UnitRepository(
        IDbContextFactory<EasyPeasyDbContext> contextFactory,
        IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    /// <inheritdoc/>
    public async Task<List<UnitEntity>> GetByCourseAsync(int courseId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Units
            .AsNoTracking()
            .Where(u => u.CourseId == courseId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Difficulty counts are computed in SQL from <see cref="EasyPeasy.Core.Extensions.RateExtensions.EasyMax"/>/
    /// <see cref="EasyPeasy.Core.Extensions.RateExtensions.HardMin"/> directly (not via
    /// <c>ToDifficulty()</c>, which isn't translatable) — kept in sync by construction since both read
    /// the same constants, but mirrors the bucketing logic by hand rather than calling it.
    /// </remarks>
    public async Task<List<UnitCardModel>> GetUnitCardsAsync(int courseId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Set<UnitEntity>()
            .AsNoTracking()
            .Where(u => u.CourseId == courseId)
            // Newest first. Imported units often share a creation timestamp, so Id breaks the tie
            // — within one import the later row is still the later unit.
            .OrderByDescending(u => u.CreatedAt)
            .ThenByDescending(u => u.Id)
            .Select(u => new UnitCardModel
            {
                Id = u.Id,
                RecordGuid = u.RecordGuid,
                Title = u.Title,
                Description = u.Description,

                WordCount = u.Words.Count,
                IrregularFormCount = u.IrregularForms.Count,
                StudyCardCount = u.StudyCards.Count,
                TestCardCount = u.TestCards.Count,

                TotalCount = u.Words.Count + u.IrregularForms.Count + u.StudyCards.Count + u.TestCards.Count,

                EasyCount = u.Words.Count(w => w.Rate < RateExtensions.EasyMax)
                + u.IrregularForms.Count(f => f.Rate < RateExtensions.EasyMax)
                + u.StudyCards.Count(c => c.Rate < RateExtensions.EasyMax)
                + u.TestCards.Count(c => c.Rate < RateExtensions.EasyMax),

                MediumCount = u.Words.Count(w => w.Rate >= RateExtensions.EasyMax && w.Rate < RateExtensions.HardMin)
                + u.IrregularForms.Count(f => f.Rate >= RateExtensions.EasyMax && f.Rate < RateExtensions.HardMin)
                + u.StudyCards.Count(c => c.Rate >= RateExtensions.EasyMax && c.Rate < RateExtensions.HardMin)
                + u.TestCards.Count(c => c.Rate >= RateExtensions.EasyMax && c.Rate < RateExtensions.HardMin),

                HardCount = u.Words.Count(w => w.Rate >= RateExtensions.HardMin)
                + u.IrregularForms.Count(f => f.Rate >= RateExtensions.HardMin)
                + u.StudyCards.Count(c => c.Rate >= RateExtensions.HardMin)
                + u.TestCards.Count(c => c.Rate >= RateExtensions.HardMin),
            })
            .ToListAsync();
    }
}
