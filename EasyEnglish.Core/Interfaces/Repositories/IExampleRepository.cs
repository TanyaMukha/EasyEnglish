using EasyEnglish.Core.Entities;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Repositories;

public interface IExampleRepository : IBaseRepository<ExampleEntity>
{
    /// <summary>All examples belonging to a unit's words, as a flat list (joined through Word, no need to load words).</summary>
    Task<List<ExampleEntity>> GetByUnitAsync(int unitId);
}
