using EasyEnglish.Core.Entities;
using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface IUnitRepository : IBaseWithGuidRepository<UnitEntity>
{
    Task<List<UnitCardModel>> GetUnitCardsAsync(int courseId);

    /// <summary>Усі юніти курсу.</summary>
    Task<List<UnitEntity>> GetByCourseAsync(int courseId);
}
