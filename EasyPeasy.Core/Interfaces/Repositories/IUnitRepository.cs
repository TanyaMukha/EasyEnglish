using EasyPeasy.Core.Entities;
using EasyPeasy.Core.Models;
using MukhaLab.Database;

namespace EasyPeasy.Core.Interfaces.Repositories;

/// <summary>Repository for <see cref="UnitEntity"/>, beyond the generic CRUD in <see cref="IBaseWithGuidRepository{T}"/>.</summary>
public interface IUnitRepository : IBaseWithGuidRepository<UnitEntity>
{
    /// <summary>
    /// Units of a course as lightweight <see cref="UnitCardModel"/> summaries (difficulty-bucketed
    /// word counts), for a course overview list without loading each unit's full content.
    /// </summary>
    Task<List<UnitCardModel>> GetUnitCardsAsync(int courseId);

    /// <summary>All units belonging to a course.</summary>
    Task<List<UnitEntity>> GetByCourseAsync(int courseId);
}
