using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

/// <summary>
/// EF Core-backed <see cref="ICourseRepository"/>. Adds no members of its own — all behavior comes
/// from <see cref="BaseWithGuidRepository{T, TContext}"/>.
/// </summary>
public class CourseRepository : BaseWithGuidRepository<CourseEntity, EasyEnglishDbContext>, ICourseRepository
{
    public CourseRepository(
        IDbContextFactory<EasyEnglishDbContext> contextFactory,
        IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }
}
