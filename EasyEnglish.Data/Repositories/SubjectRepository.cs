using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

public class SubjectRepository : BaseRepository<SubjectEntity, EasyEnglishDbContext>, ISubjectRepository
{
    public SubjectRepository(IDbContextFactory<EasyEnglishDbContext> contextFactory, IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }

    /// <inheritdoc/>
    public async Task<int> CountCoursesAsync(int subjectId)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Courses.CountAsync(c => c.SubjectId == subjectId);
    }
}
