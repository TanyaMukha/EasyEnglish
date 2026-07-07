using EasyEnglish.Core.Models;
using MukhaLab.Database;

namespace EasyEnglish.Core.Interfaces.Services;

public interface IExampleService : IBaseService<ExampleModel>
{
    /// <summary>All examples belonging to a unit's words, as a flat list.</summary>
    Task<IEnumerable<ExampleModel>> GetByUnitAsync(int unitId);
}
