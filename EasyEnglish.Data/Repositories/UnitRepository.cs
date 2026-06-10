using AutoMapper;
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
        IMapper mapper,
        IDbContextFactory<EasyEnglishDbContext> contextFactory,
        IUserContext? userContext = null)
        : base(mapper, contextFactory, userContext)
    {
    }

    public async Task<List<UnitCardModel>> GetCardsAsync(int courseId)
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
                TotalWordsCount = u.Words.Count,
                EasyWordsCount = u.Words.Count(w => w.Rate < RateExtensions.EasyMax),
                MediumWordsCount = u.Words.Count(w => w.Rate >= RateExtensions.EasyMax
                                                   && w.Rate < RateExtensions.HardMin),
                HardWordsCount = u.Words.Count(w => w.Rate >= RateExtensions.HardMin),
            })
            .ToListAsync();
    }
}
