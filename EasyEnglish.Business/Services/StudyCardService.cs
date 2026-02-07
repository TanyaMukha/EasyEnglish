using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using EasyEnglish.Core.Interfaces.Services;
using EasyEnglish.Core.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MukhaLab.SelectQueryParameters.Models;

namespace EasyEnglish.Services.Services;

public class StudyCardService : BaseService<StudyCardEntity, StudyCardModel>, IStudyCardService
{
    public StudyCardService(
        IStudyCardRepository repository,
        IMapper mapper,
        ILogger<StudyCardService> logger)
        : base(repository, mapper, logger)
    {
    }
}