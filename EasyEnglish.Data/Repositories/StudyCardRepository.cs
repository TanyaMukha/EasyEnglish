using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;

namespace EasyEnglish.Data.Repositories;

public class StudyCardRepository : BaseRepository<StudyCardEntity, EasyEnglishDbContext>, IStudyCardRepository
{
    public StudyCardRepository(IDbContextFactory<EasyEnglishDbContext> contextFactory, IUserContext userContext)
        : base(contextFactory, userContext)
    {
    }
}
