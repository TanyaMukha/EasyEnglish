using EasyPeasy.Core.Models;
using MukhaLab.Database;

namespace EasyPeasy.Core.Interfaces.Services;

/// <summary>Service for <see cref="ExampleModel"/>, beyond the generic CRUD in <see cref="IBaseService{TModel}"/>.</summary>
public interface IExampleService : IBaseService<ExampleModel>
{
    /// <summary>All examples belonging to a unit's words, as a flat list.</summary>
    Task<IEnumerable<ExampleModel>> GetByUnitAsync(int unitId);
}
