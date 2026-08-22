using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyPeasy.Data.Repositories;

public class SubjectRepository : BaseRepository<SubjectEntity, EasyPeasyDbContext>, ISubjectRepository
{
    public SubjectRepository(IDbContextFactory<EasyPeasyDbContext> contextFactory, IUserContext? userContext = null)
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
