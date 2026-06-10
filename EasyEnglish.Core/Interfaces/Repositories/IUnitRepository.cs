using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface IUnitRepository : IBaseWithGuidRepository<UnitEntity>
{
    Task<List<UnitCardModel>> GetCardsAsync(int courseId);
}
