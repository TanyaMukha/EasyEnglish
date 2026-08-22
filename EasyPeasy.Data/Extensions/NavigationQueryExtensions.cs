using Microsoft.EntityFrameworkCore;

namespace EasyPeasy.Data.Extensions;

/// <summary>Shared cyclic prev/next navigation logic, used by every repository's <c>GetNavigationIdsAsync</c>.</summary>
public static class NavigationQueryExtensions
{
    /// <summary>
    /// Given a query already filtered/ordered/projected down to the candidate ids (e.g.
    /// <c>ctx.Words.Where(w => w.UnitId == unitId).OrderBy(w => w.Id).Select(w => w.Id)</c>), finds
    /// <paramref name="currentId"/>'s neighbors with cyclic wraparound (last id's "next" is the first,
    /// first id's "previous" is the last), plus its 1-based position and the total candidate count.
    /// If <paramref name="currentId"/> isn't among <paramref name="orderedIds"/>, both neighbors are
    /// <c>null</c> and position is <c>0</c> (total is still the real candidate count).
    /// </summary>
    public static async Task<(int? PreviousId, int? NextId, int Position, int Total)> GetCyclicNavigationAsync(
        this IQueryable<int> orderedIds, int currentId)
    {
        var ids = await orderedIds.ToListAsync();

        var currentIndex = ids.IndexOf(currentId);
        if (currentIndex == -1)
            return (null, null, 0, ids.Count);

        var previousId = ids.Count > 1
            ? ids[(currentIndex - 1 + ids.Count) % ids.Count]
            : (int?)null;
        var nextId = ids.Count > 1
            ? ids[(currentIndex + 1) % ids.Count]
            : (int?)null;

        return (previousId, nextId, currentIndex + 1, ids.Count);
    }
}
