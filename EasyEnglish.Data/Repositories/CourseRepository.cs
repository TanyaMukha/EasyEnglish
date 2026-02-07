using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

public class CourseRepository : BaseRepository<CourseEntity, EasyEnglishDbContext>, ICourseRepository
{
    public CourseRepository(
        IMapper mapper, 
        IDbContextFactory<EasyEnglishDbContext> contextFactory,
        IUserContext? userContext = null) 
        : base(mapper, contextFactory, userContext)
    {
        ConfigureIncludes(new[] { "Units" });
    }
}
