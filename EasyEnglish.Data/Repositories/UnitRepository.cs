using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using MukhaLab.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyEnglish.Data.Repositories;

public class UnitRepository : BaseRepository<UnitEntity, EasyEnglishDbContext>, IUnitRepository
{
    public UnitRepository(
        IMapper mapper,
        IDbContextFactory<EasyEnglishDbContext> contextFactory,
        IUserContext? userContext = null)
        : base(mapper, contextFactory, userContext)
    {
        ConfigureIncludes(new[] { "Words", "Course", "IrregularForms" });
    }
}
