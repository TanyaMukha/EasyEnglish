using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;
using System.Linq.Expressions;

namespace EasyEnglish.Data.Repositories;

public class IrregularFormRepository : BaseRepository<IrregularFormEntity, EasyEnglishDbContext>, IIrregularFormRepository
{
    /// <summary>
    /// Gets navigation property paths to be included by default when querying Tag entities.
    /// </summary>
    private static readonly string[] RelativedEntities = new[] { "Unit" };

    public IrregularFormRepository(IMapper mapper, EasyEnglishDbContext context, IUserContext userContext)
        : base(mapper, context, userContext)
    {
        this.ConfigureIncludes(RelativedEntities);
    }
}
