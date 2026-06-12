using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Extensions;

namespace EasyEnglish.Data.Repositories;

public class UnitRepository : BaseWithGuidRepository<UnitEntity, EasyEnglishDbContext>, IUnitRepository
{
    public UnitRepository(
        IDbContextFactory<EasyEnglishDbContext> contextFactory,
        IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    public async Task<List<UnitEntity>> GetByCourseAsync(int courseId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Units
            .AsNoTracking()
            .Where(u => u.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<List<UnitCardModel>> GetUnitCardsAsync(int courseId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Set<UnitEntity>()
            .AsNoTracking()
            .Where(u => u.CourseId == courseId)
            .OrderBy(u => u.Id)
            .Select(u => new UnitCardModel
            {
                Id = u.Id,
                RecordGuid = u.RecordGuid,
                Title = u.Title,
                Description = u.Description,

                TotalCount = u.Words.Count + u.IrregularForms.Count,

                EasyCount = u.Words.Count(w => w.Rate < RateExtensions.EasyMax)
                + u.IrregularForms.Count(f => f.Rate < RateExtensions.EasyMax),

                MediumCount = u.Words.Count(w => w.Rate >= RateExtensions.EasyMax && w.Rate < RateExtensions.HardMin)
                + u.IrregularForms.Count(f => f.Rate >= RateExtensions.EasyMax && f.Rate < RateExtensions.HardMin),

                HardCount = u.Words.Count(w => w.Rate >= RateExtensions.HardMin)
                + u.IrregularForms.Count(f => f.Rate >= RateExtensions.HardMin),
            })
            .ToListAsync();
    }
}
