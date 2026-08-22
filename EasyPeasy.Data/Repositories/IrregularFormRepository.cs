using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Interfaces.Repositories;
using EasyPeasy.Core.Options;
using EasyPeasy.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyPeasy.Data.Repositories;

/// <summary>
/// EF Core-backed <see cref="IIrregularFormRepository"/>. Unlike its siblings
/// (<see cref="WordRepository"/>, <see cref="StudyCardRepository"/>, <see cref="TestCardRepository"/>),
/// this repository has no <c>GetNavigationIdsAsync</c> — <see cref="IIrregularFormRepository"/> simply
/// doesn't declare one, so there's no prev/next navigation UI for irregular forms.
/// </summary>
public class IrregularFormRepository : BaseRepository<IrregularFormEntity, EasyPeasyDbContext>, IIrregularFormRepository
{
    public IrregularFormRepository(IDbContextFactory<EasyPeasyDbContext> contextFactory, IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    /// <inheritdoc/>
    public async Task<List<IrregularFormEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        IQueryable<IrregularFormEntity> query = ctx.IrregularForms
            .Where(f => f.Unit!.CourseId == courseId);

        if (unitId is not null)
            query = query.Where(f => f.UnitId == unitId);

        return await query.AsNoTracking().ApplyLearningSelectionAsync(options);
    }

    /// <inheritdoc/>
    public async Task<int> CountReviewedSinceAsync(DateTime since)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.IrregularForms.CountAsync(f => f.LastReviewDate >= since);
    }
}
