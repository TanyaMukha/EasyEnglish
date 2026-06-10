using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;
using System.Linq.Expressions;

namespace EasyEnglish.Data.Repositories;

public class IrregularFormRepository : BaseRepository<IrregularFormEntity, EasyEnglishDbContext>, IIrregularFormRepository
{
    public IrregularFormRepository(IMapper mapper, IDbContextFactory<EasyEnglishDbContext> contextFactory, IUserContext userContext)
        : base(mapper, contextFactory, userContext)
    {
    }
}
