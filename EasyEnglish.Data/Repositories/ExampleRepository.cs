using AutoMapper;
using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Interfaces.Repositories;
using MukhaLab.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyEnglish.Data.Repositories;

public class ExampleRepository : BaseRepository<ExampleEntity, EasyEnglishDbContext>, IExampleRepository
{
    public ExampleRepository(IMapper mapper, EasyEnglishDbContext context, IUserContext userContext)
        : base(mapper, context, userContext)
    {
    }
}
