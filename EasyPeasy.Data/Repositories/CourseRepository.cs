using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyPeasy.Data.Repositories;

/// <summary>
/// EF Core-backed <see cref="ICourseRepository"/>. Adds no members of its own — all behavior comes
/// from <see cref="BaseWithGuidRepository{T, TContext}"/>.
/// </summary>
public class CourseRepository : BaseWithGuidRepository<CourseEntity, EasyPeasyDbContext>, ICourseRepository
{
    public CourseRepository(
        IDbContextFactory<EasyPeasyDbContext> contextFactory,
        IUserContext? userContext = null)
        : base(contextFactory, userContext)
    {
    }
}
