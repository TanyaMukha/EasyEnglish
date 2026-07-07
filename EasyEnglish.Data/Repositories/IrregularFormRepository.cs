using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using EasyEnglish.Core.Options;
using EasyEnglish.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

public class IrregularFormRepository : BaseRepository<IrregularFormEntity, EasyEnglishDbContext>, IIrregularFormRepository
{
    public IrregularFormRepository(IDbContextFactory<EasyEnglishDbContext> contextFactory, IUserContext userContext)
        : base(contextFactory, userContext)
    {
    }

    public async Task<List<IrregularFormEntity>> GetForLearningAsync(int courseId, int? unitId, LearningSelectionOptions options)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        IQueryable<IrregularFormEntity> query = ctx.IrregularForms
            .Where(f => f.Unit!.CourseId == courseId);

        if (unitId is not null)
            query = query.Where(f => f.UnitId == unitId);

        query = query.ApplyLearningSelection(options);

        return await query.AsNoTracking().ToListAsync();
    }
}
