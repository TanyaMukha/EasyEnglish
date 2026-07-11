using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.Database;

/// <summary>
/// Base implementation of a repository providing CRUD operations.
/// Thread-safe. Each operation creates and disposes its own DbContext via IDbContextFactory.
/// </summary>
/// <typeparam name="T">The type of the entity. Must derive from <see cref="AbstractEntity"/>.</typeparam>
/// <typeparam name="TContext">The type of the DbContext.</typeparam>
/// <remarks>
/// <b>Per-user filtering</b> is applied by every method once both a non-null <see cref="IUserContext"/>
/// is supplied to the constructor and <see cref="ConfigureUserIdField"/> has been called (see
/// <see cref="IsUserFilteringActive"/>). Methods that operate on a single or a batch of primary keys
/// (<see cref="FindAsync(int, string[], CancellationToken)"/>, <see cref="FindManyAsync(IEnumerable{int}, string[], CancellationToken)"/>,
/// <see cref="UpdateAsync"/>, <see cref="RemoveAsync"/>, every <c>RemoveRangeAsync</c> overload) verify
/// that every affected id is visible under the active filter and throw
/// <see cref="EntityNotFoundException"/> otherwise, rather than silently allowing access to another
/// user's rows. <see cref="FindAsync(object[])"/>'s composite-key branch (used when the primary key
/// is not a single <see cref="int"/>) is the one exception: the filter cannot be composed with a raw
/// <c>DbSet.FindAsync(object[])</c> call, so that branch is not scoped.
/// </remarks>
public abstract class BaseRepository<T, TContext> : IBaseRepository<T>
    where T : AbstractEntity
    where TContext : DbContext
{
    protected readonly IDbContextFactory<TContext> contextFactory;
    protected readonly IUserContext? userContext;
    protected string[] userIdPropertyPaths = Array.Empty<string>();
    protected bool enableUserFiltering = false;

    /// <summary>
    /// True when per-user row scoping is both enabled (a non-null <see cref="IUserContext"/> was
    /// supplied to the constructor) and actually configured (<see cref="ConfigureUserIdField"/> was
    /// called with at least one property path). Supplying an <see cref="IUserContext"/> alone is not
    /// enough to activate scoping — see <see cref="AnonymousUserContext"/>'s remarks for why that
    /// distinction matters.
    /// </summary>
    protected bool IsUserFilteringActive => this.enableUserFiltering && this.userIdPropertyPaths is { Length: > 0 };

    /// <summary>Initializes the repository.</summary>
    /// <param name="contextFactory">Factory used to create a fresh <typeparamref name="TContext"/> per operation.</param>
    /// <param name="userContext">
    /// Optional per-user identity source. When non-null, per-user row scoping is enabled (see
    /// <see cref="IncludeUserIdFilter"/>) — but only takes effect once
    /// <see cref="ConfigureUserIdField"/> is also called with the entity's user-id property path(s).
    /// </param>
    public BaseRepository(
        IDbContextFactory<TContext> contextFactory,
        IUserContext? userContext = null)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);

        this.contextFactory = contextFactory;
        this.userContext = userContext;
        this.enableUserFiltering = userContext != null;
    }

    /// <summary>Applies an <c>Include</c> for every non-blank, distinct path in <paramref name="paths"/>.</summary>
    private IQueryable<T> ApplyIncludes(IQueryable<T> query, IEnumerable<string> paths)
    {
        if (paths is not null)
            foreach (var include in paths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct())
                query = query.Include(include);
        return query;
    }

    /// <summary>
    /// Base select query with the per-user filter and eager-loaded includes applied.
    /// Derived repositories use this as the starting point for their own LINQ queries.
    /// </summary>
    /// <param name="ctx">The DbContext to query against.</param>
    /// <param name="includes">Navigation property paths to eager-load via <c>Include</c>.</param>
    protected IQueryable<T> BuildSelectQuery(TContext ctx, string[]? includes = null)
    {
        IQueryable<T> query = ctx.Set<T>();

        query = this.enableUserFiltering ? this.IncludeUserIdFilter(query) : query;
        query = ApplyIncludes(query, includes);

        return query;
    }

    /// <summary>
    /// Select query with dynamic filtering, sorting, and paging applied via
    /// <c>MukhaLab.SelectQueryParameters</c>, on top of <see cref="BuildSelectQuery(TContext, string[])"/>.
    /// </summary>
    /// <param name="ctx">The DbContext to query against.</param>
    /// <param name="parameters">Filtering, sorting, and paging instructions.</param>
    /// <param name="withoutPagination">When <c>true</c>, <see cref="QueryParameters.PageNumber"/>/<see cref="QueryParameters.RowCount"/> are ignored (used for count queries).</param>
    /// <param name="includes">Navigation property paths to eager-load via <c>Include</c>.</param>
    private IQueryable<T> BuildSelectQuery(
        TContext ctx,
        QueryParameters parameters,
        bool withoutPagination = false,
        string[]? includes = null)
    {
        var queryParameters = withoutPagination ? new QueryParameters
        {
            Filters = parameters.Filters,
            Sort = parameters.Sort,
            PageNumber = null,
            RowCount = null
        } : parameters;

        return BuildSelectQuery(ctx, includes).ApplyQueryParameters(queryParameters);
    }

    /// <summary>Checks whether the entity with the given id is visible under the active per-user filter. Always true when filtering isn't active.</summary>
    private async Task<bool> IsOwnedAsync(TContext ctx, int id, CancellationToken cancellationToken = default)
    {
        if (!this.IsUserFilteringActive)
            return true;

        return await this.IncludeUserIdFilter(ctx.Set<T>().AsNoTracking())
            .AnyAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>Throws <see cref="EntityNotFoundException"/> if any id in <paramref name="ids"/> is not visible under the active per-user filter. No-op when filtering isn't active.</summary>
    private async Task EnsureAllOwnedAsync(TContext ctx, IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
    {
        if (!this.IsUserFilteringActive || ids.Count == 0)
            return;

        var ownedIds = (await this.IncludeUserIdFilter(ctx.Set<T>().AsNoTracking())
            .Where(e => ids.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken)).ToHashSet();

        var missing = ids.Where(id => !ownedIds.Contains(id)).ToList();
        if (missing.Count > 0)
        {
            throw new EntityNotFoundException(
                $"Entities of type {typeof(T).Name} were not all found: id(s) {string.Join(", ", missing)} do not exist or are not owned by the current user.");
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> GetAsync(string[]? includes = null, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = BuildSelectQuery(ctx, includes);
        var entities = await query.AsNoTracking().ToListAsync(cancellationToken);
        return entities;
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> GetAsync(QueryParameters parameters, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = BuildSelectQuery(ctx, parameters, false, includes);
        var entities = await query.AsNoTracking().ToListAsync(cancellationToken);
        return entities;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Applies the same per-user filter as <see cref="GetAsync(string[], CancellationToken)"/> so the
    /// count matches what a caller would see in the data, but does not accept a
    /// <see cref="QueryParameters"/> filter — use <see cref="GetPaginationInfoAsync"/> for a filtered count.
    /// </remarks>
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Same per-user filter as the select queries, so the count matches the data.
        IQueryable<T> query = ctx.Set<T>();
        query = this.enableUserFiltering ? this.IncludeUserIdFilter(query) : query;

        return await query.CountAsync(cancellationToken);
    }

    /// <summary>Computes the number of pages of size <paramref name="countPerPage"/> needed to cover <paramref name="totalCount"/> rows.</summary>
    private int GetPageCount(int totalCount, int countPerPage)
    {
        return countPerPage > 0
            ? (totalCount + countPerPage - 1) / countPerPage
            : 1;
    }

    /// <inheritdoc/>
    public virtual async Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        int countPerPage = parameters.RowCount ?? 0;

        // No includes: they have no effect on a COUNT query.
        var query = this.BuildSelectQuery(ctx, parameters, withoutPagination: true, includes: Array.Empty<string>());
        var totalCount = await query.CountAsync(cancellationToken);

        return new PaginationInfo
        {
            TotalCount = totalCount,
            TotalPages = this.GetPageCount(totalCount, countPerPage),
        };
    }

    /// <inheritdoc/>
    public virtual async Task<T?> FindAsync(int id, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);

        var includesToApply = includes ?? [];

        if (includesToApply.Length == 0 && !this.IsUserFilteringActive)
        {
            var entity = await ctx.Set<T>().FindAsync([id], cancellationToken);
            if (entity != null)
                ctx.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        IQueryable<T> query = ApplyIncludes(ctx.Set<T>(), includesToApply);
        query = this.IsUserFilteringActive ? this.IncludeUserIdFilter(query) : query;
        return await query.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The composite-key branch (any <paramref name="keyValues"/> shape other than a single
    /// <see cref="int"/>) does not apply the per-user filter — a raw <c>DbSet.FindAsync(object[])</c>
    /// call cannot be composed with a <c>Where</c> predicate. The single-<see cref="int"/>-key branch
    /// delegates to <see cref="FindAsync(int, string[], CancellationToken)"/>, which is filtered.
    /// </remarks>
    public virtual async Task<T?> FindAsync(params object[] keyValues)
    {
        if (keyValues == null || keyValues.Length == 0)
            return null;

        // Single int key: delegate to the overload that supports includes and per-user filtering.
        if (keyValues.Length == 1 && keyValues[0] is int id)
            return await FindAsync(id);

        // Composite keys: includes and per-user filtering are not supported here.
        await using var ctx = await contextFactory.CreateDbContextAsync();
        var result = await ctx.Set<T>().FindAsync(keyValues);
        if (result != null)
            ctx.Entry(result).State = EntityState.Detached;
        return result;
    }

    /// <inheritdoc/>
    public virtual async Task<List<T>> FindManyAsync(IEnumerable<int> ids, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any())
            return new List<T>();

        var idList = ids.ToList();
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = ApplyIncludes(ctx.Set<T>(), includes);
        query = this.IsUserFilteringActive ? this.IncludeUserIdFilter(query) : query;

        return await query.AsNoTracking()
            .Where(e => idList.Contains(e.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<List<T>> FindManyAsync(params int[] ids)
    {
        if (ids == null || ids.Length == 0)
            return new List<T>();

        return await FindManyAsync((IEnumerable<int>)ids);
    }

    /// <inheritdoc/>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        ctx.Set<T>().Add(entity);
        await ctx.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        ctx.Set<T>().AddRange(entities);
        await ctx.SaveChangesAsync(cancellationToken);
        return entities;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <see cref="Microsoft.EntityFrameworkCore.DbContext.Update{TEntity}(TEntity)"/> marks every
    /// property as modified, so the full entity state is written regardless of which fields actually
    /// changed — there is no field-level merge. If the row was concurrently deleted (or, for an
    /// entity with an EF Core concurrency token configured, concurrently modified),
    /// <see cref="DbUpdateConcurrencyException"/> is caught and rethrown as
    /// <see cref="EntityNotFoundException"/>. When per-user filtering is active, the entity's
    /// ownership is verified before the update is attempted.
    /// </remarks>
    /// <exception cref="EntityNotFoundException">
    /// The entity does not exist, is not owned by the current user (when per-user filtering is
    /// active), or was concurrently modified/deleted.
    /// </exception>
    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);

        if (!await this.IsOwnedAsync(ctx, entity.Id, cancellationToken))
        {
            throw new EntityNotFoundException(
                $"Entity of type {typeof(T).Name} with id {entity.Id} was not found.");
        }

        ctx.Set<T>().Update(entity);

        try
        {
            int result = await ctx.SaveChangesAsync(cancellationToken);
            if (result == 0)
            {
                throw new EntityNotFoundException(
                    $"Entity of type {typeof(T).Name} was not updated — no rows were affected (the record may not exist).");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new EntityNotFoundException(
                $"Entity of type {typeof(T).Name} with id {entity.Id} was modified or deleted by another operation before this update could be applied.",
                ex);
        }

        return entity;
    }

    /// <inheritdoc/>
    /// <remarks>Same full-entity-overwrite and concurrency-handling behavior as <see cref="UpdateAsync"/>, applied to each entity.</remarks>
    /// <exception cref="EntityNotFoundException">
    /// One or more entities do not exist, are not owned by the current user (when per-user filtering
    /// is active), or were concurrently modified/deleted.
    /// </exception>
    public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entityList = entities as IReadOnlyCollection<T> ?? entities.ToList();
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);

        await this.EnsureAllOwnedAsync(ctx, entityList.Select(e => e.Id).ToList(), cancellationToken);

        ctx.Set<T>().UpdateRange(entityList);

        try
        {
            await ctx.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new EntityNotFoundException(
                $"One or more entities of type {typeof(T).Name} were modified or deleted by another operation before this update could be applied.",
                ex);
        }

        return entityList;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// When per-user filtering is active, the entity's ownership is verified before it is removed.
    /// </remarks>
    /// <exception cref="EntityNotFoundException">
    /// The entity does not exist, is not owned by the current user (when per-user filtering is
    /// active), or was concurrently modified/deleted.
    /// </exception>
    public virtual async Task<bool> RemoveAsync(params object[] keyValues)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var entity = await ctx.Set<T>().FindAsync(keyValues);
        if (entity == null || !await this.IsOwnedAsync(ctx, entity.Id))
        {
            throw new EntityNotFoundException(
                $"Entity of type {typeof(T).Name} with keys {string.Join(", ", keyValues)} was not found.");
        }

        ctx.Set<T>().Remove(entity);

        try
        {
            int res = await ctx.SaveChangesAsync();
            return res > 0;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new EntityNotFoundException(
                $"Entity of type {typeof(T).Name} with keys {string.Join(", ", keyValues)} was modified or deleted by another operation before this delete could be applied.",
                ex);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// All-or-nothing: if any id in <paramref name="ids"/> doesn't match an existing, currently-owned
    /// row, nothing is deleted.
    /// </remarks>
    /// <exception cref="EntityNotFoundException">
    /// One or more ids do not exist, are not owned by the current user (when per-user filtering is
    /// active), or were concurrently modified/deleted.
    /// </exception>
    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids?.Distinct().ToList() ?? [];
        if (idList.Count == 0)
            return false;

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<T> query = ctx.Set<T>();
        query = this.enableUserFiltering ? this.IncludeUserIdFilter(query) : query;

        var entities = await query
            .Where(e => idList.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (entities.Count != idList.Count)
        {
            throw new EntityNotFoundException(
                $"Entities of type {typeof(T).Name} were not all found: expected {idList.Count}, found {entities.Count}.");
        }

        ctx.Set<T>().RemoveRange(entities);

        try
        {
            int res = await ctx.SaveChangesAsync(cancellationToken);
            return res > 0;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new EntityNotFoundException(
                $"One or more entities of type {typeof(T).Name} were modified or deleted by another operation before this delete could be applied.",
                ex);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Issues one lookup per key set (not a single batched query). When per-user filtering is active,
    /// ownership of every found entity is verified before any are removed.
    /// </remarks>
    /// <exception cref="EntityNotFoundException">
    /// One or more entities do not exist, are not owned by the current user (when per-user filtering
    /// is active), or were concurrently modified/deleted.
    /// </exception>
    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<object[]> keyValuesList, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entities = new List<T>();

        foreach (var keyValues in keyValuesList)
        {
            var entity = await ctx.Set<T>().FindAsync(keyValues, cancellationToken);
            if (entity == null)
            {
                throw new EntityNotFoundException(
                    $"Entity of type {typeof(T).Name} with keys {string.Join(", ", keyValues)} was not found.");
            }
            entities.Add(entity);
        }

        await this.EnsureAllOwnedAsync(ctx, entities.Select(e => e.Id).ToList(), cancellationToken);

        ctx.Set<T>().RemoveRange(entities);

        try
        {
            int res = await ctx.SaveChangesAsync(cancellationToken);
            return res > 0;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new EntityNotFoundException(
                $"One or more entities of type {typeof(T).Name} were modified or deleted by another operation before this delete could be applied.",
                ex);
        }
    }

    /// <summary>
    /// Deletes already-loaded <paramref name="entities"/> in a single <c>SaveChanges</c> call. When
    /// per-user filtering is active, ownership of every entity is verified before any are removed.
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="EntityNotFoundException">
    /// One or more entities are not owned by the current user (when per-user filtering is active),
    /// or were concurrently modified/deleted.
    /// </exception>
    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null || !entities.Any())
            return false;

        var entityList = entities as IReadOnlyCollection<T> ?? entities.ToList();
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);

        await this.EnsureAllOwnedAsync(ctx, entityList.Select(e => e.Id).ToList(), cancellationToken);

        ctx.Set<T>().RemoveRange(entityList);

        try
        {
            int res = await ctx.SaveChangesAsync(cancellationToken);
            return res > 0;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new EntityNotFoundException(
                $"One or more entities of type {typeof(T).Name} were modified or deleted by another operation before this delete could be applied.",
                ex);
        }
    }

    /// <summary>
    /// Retrieves an entity by primary key value(s) and throws if it does not exist.
    /// </summary>
    /// <param name="keyValues">The primary key value(s) of the entity to retrieve.</param>
    /// <exception cref="EntityNotFoundException">No entity matches <paramref name="keyValues"/>.</exception>
    protected virtual async Task<T> GetByIdAndEnsureExistsAsync(params object[] keyValues)
    {
        var entity = await this.FindAsync(keyValues);
        return entity ?? throw new EntityNotFoundException(
            $"Entity of type {typeof(T).Name} with keys {string.Join(", ", keyValues)} was not found.");
    }

    /// <summary>
    /// Adds <paramref name="entity"/> to <paramref name="ctx"/> without saving. For composing with an
    /// externally-supplied context, e.g. inside an <see cref="ExecuteInTransactionAsync{TResult}"/>
    /// callback, where <see cref="AddAsync"/> cannot be used because it opens its own context.
    /// </summary>
    protected void Add(TContext ctx, T entity) => ctx.Set<T>().Add(entity);

    /// <summary>
    /// Marks <paramref name="entity"/> as modified on <paramref name="ctx"/> without saving. For
    /// composing with an externally-supplied context; see <see cref="Add(TContext, T)"/>. Does not
    /// perform the per-user ownership check that <see cref="UpdateAsync"/> does — callers using this
    /// directly are responsible for authorization.
    /// </summary>
    protected void Update(TContext ctx, T entity) => ctx.Set<T>().Update(entity);

    /// <summary>
    /// Marks <paramref name="entity"/> as removed on <paramref name="ctx"/> without saving. For
    /// composing with an externally-supplied context; see <see cref="Add(TContext, T)"/>. Does not
    /// perform the per-user ownership check that <see cref="RemoveAsync"/> does — callers using this
    /// directly are responsible for authorization.
    /// </summary>
    protected void Remove(TContext ctx, T entity) => ctx.Set<T>().Remove(entity);

    /// <summary>
    /// Executes multiple operations in a single transaction.
    /// Use when you need atomicity across several SaveChanges calls within one repository.
    /// </summary>
    /// <param name="operation">
    /// The work to run inside the transaction, given the transaction's own <typeparamref name="TContext"/>.
    /// Perform every read/write via this <c>ctx</c> parameter directly (e.g. <c>ctx.Set&lt;T&gt;()</c>,
    /// <c>ctx.SaveChangesAsync()</c>, or the <see cref="Add(TContext, T)"/>/<see cref="Update(TContext, T)"/>/<see cref="Remove(TContext, T)"/>
    /// helpers). Calling back into other repository methods (which each open their own
    /// <typeparamref name="TContext"/> via the factory) runs those calls on a separate connection
    /// outside this transaction.
    /// </param>
    /// <returns>The value returned by <paramref name="operation"/>.</returns>
    /// <exception cref="Exception">Any exception thrown by <paramref name="operation"/> triggers a rollback and is rethrown.</exception>
    public virtual async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<TContext, Task<TResult>> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var ctx = await contextFactory.CreateDbContextAsync();
        await using var transaction = await ctx.Database.BeginTransactionAsync();
        try
        {
            var result = await operation(ctx);
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Executes multiple operations in a single transaction without a return value. See
    /// <see cref="ExecuteInTransactionAsync{TResult}"/> for the same caveat about using the supplied
    /// <c>ctx</c> directly rather than calling back into other repository methods.
    /// </summary>
    /// <param name="operation">The work to run inside the transaction.</param>
    public virtual async Task ExecuteInTransactionAsync(Func<TContext, Task> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteInTransactionAsync<object?>(async ctx =>
        {
            await operation(ctx);
            return null;
        });
    }

    /// <summary>
    /// Configures which entity property path(s) hold the owning user's id, enabling per-user
    /// filtering in <see cref="IncludeUserIdFilter"/>. Has no effect unless the repository was also
    /// constructed with a non-null <see cref="IUserContext"/> (see <see cref="IsUserFilteringActive"/>).
    /// </summary>
    /// <param name="userIdPropertyPaths">
    /// One or more dot-separated property paths identifying the owning user (e.g. <c>"UserId"</c>, or
    /// <c>"Owner.UserId"</c> for a collection navigation — see <see cref="IncludeUserIdFilter"/> for
    /// the collection-path syntax). Multiple paths are OR-combined: a row matches if it satisfies
    /// <em>any</em> of them.
    /// </param>
    public void ConfigureUserIdField(string[] userIdPropertyPaths)
    {
        this.userIdPropertyPaths = userIdPropertyPaths;
    }

    /// <summary>
    /// Applies an OR-combined equality filter across every path configured via
    /// <see cref="ConfigureUserIdField"/>, comparing each resolved property against
    /// <see cref="IUserContext.GetCurrentUserId"/>. A no-op if no paths are configured.
    /// </summary>
    /// <param name="query">The query to filter.</param>
    /// <returns>The filtered query, or the original query unchanged if no user-id paths are configured.</returns>
    /// <remarks>
    /// Each path is resolved segment-by-segment via reflection. A segment whose property type is an
    /// <see cref="System.Collections.IEnumerable"/> (other than <see cref="string"/>) is treated as a
    /// collection navigation: the remainder of the path is resolved against the collection's element
    /// type inside an <c>Any(...)</c> call (e.g. <c>"Executors.UserId"</c> becomes
    /// <c>x.Executors.Any(e =&gt; e.UserId == currentUserId)</c>), and only a single collection hop
    /// per path is supported — the path stops resolving after the first collection segment.
    /// Non-collection segments are resolved as plain property access and compared directly
    /// (<c>x.UserId == currentUserId</c>). Assumes the resolved property is directly comparable to
    /// <see cref="Guid"/> (the type returned by <see cref="IUserContext.GetCurrentUserId"/>); a
    /// mismatched property type throws when the expression tree is built.
    /// </remarks>
    /// <exception cref="InvalidOperationException">A configured path could not be resolved against <typeparamref name="T"/>.</exception>
    protected virtual IQueryable<T> IncludeUserIdFilter(IQueryable<T> query)
    {
        if (this.userIdPropertyPaths == null || this.userIdPropertyPaths.Length == 0)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var userIdConst = Expression.Constant(this.userContext!.GetCurrentUserId());
        Expression? combined = null;

        foreach (var path in this.userIdPropertyPaths)
        {
            Expression current = parameter;
            Type currentType = typeof(T);

            var parts = path.Split('.');
            Expression? innerExpression = null;

            try
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    var prop = currentType.GetProperty(parts[i]);

                    ArgumentNullException.ThrowIfNull(prop);

                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                    {
                        var elementType = prop.PropertyType.GetGenericArguments().FirstOrDefault();
                        if (elementType == null)
                        {
                            break;
                        }

                        var itemParam = Expression.Parameter(elementType, "e");
                        var innerProp = parts[i + 1];
                        var itemProp = Expression.PropertyOrField(itemParam, innerProp);
                        var condition = Expression.Equal(itemProp, userIdConst);
                        var anyLambda = Expression.Lambda(condition, itemParam);

                        var collection = Expression.PropertyOrField(current, parts[i]);
                        var anyCall = Expression.Call(typeof(Enumerable), "Any", new[] { elementType }, collection, anyLambda);

                        innerExpression = anyCall;
                        break;
                    }
                    else
                    {
                        current = Expression.PropertyOrField(current, parts[i]);
                        currentType = prop.PropertyType;
                    }
                }

                innerExpression ??= Expression.Equal(current, userIdConst);
                combined = combined == null ? innerExpression : Expression.OrElse(combined, innerExpression);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse user-id property path '{path}': {ex.Message}", ex);
            }
        }

        if (combined == null)
        {
            return query;
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(lambda);
    }
}
