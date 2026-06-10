using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.Database;

/// <summary>
/// Base implementation of a repository providing CRUD operations.
/// Thread-safe. Each operation creates and disposes its own DbContext via IDbContextFactory.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
/// <typeparam name="TContext">The type of the DbContext.</typeparam>
public abstract class BaseRepository<T, TContext> : IBaseRepository<T>
    where T : class
    where TContext : DbContext
{
    protected readonly IDbContextFactory<TContext> contextFactory;
    protected readonly IMapper mapper;
    protected readonly IUserContext? userContext;
    protected string[] userIdPropertyPaths = Array.Empty<string>();
    protected bool enableUserFiltering = false;

    public BaseRepository(
        IMapper mapper,
        IDbContextFactory<TContext> contextFactory,
        IUserContext? userContext = null)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);

        this.mapper = mapper;
        this.contextFactory = contextFactory;
        this.userContext = userContext;
        this.enableUserFiltering = userContext != null;
    }

    private IQueryable<T> ApplyIncludes(IQueryable<T> query, IEnumerable<string> paths)
    {
        if (paths is not null)
            foreach (var include in paths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct())
                query = query.Include(include);
        return query;
    }

    private IQueryable<T> BuildSelectQuery(
    TContext ctx,
    QueryParameters parameters,
    bool withoutPagination = false,
    string[]? includes = null)
    {
        IQueryable<T> query = ctx.Set<T>();

        var queryParameters = withoutPagination ? new QueryParameters
        {
            Filters = parameters.Filters,
            Sort = parameters.Sort,
            PageNumber = null,
            RowCount = null
        } : parameters;

        query = this.enableUserFiltering ? this.IncludeUserIdFilter(query) : query;
        query = ApplyIncludes(query, includes);   // <-- было if (includeRelatedEntities)
        query = query.ApplyQueryParameters(queryParameters);

        return query;
    }

    /// <summary>
    /// Retrieves all entities as list asynchronously.
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAsync(
    QueryParameters parameters, string[]? includes = null)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        await using var ctx = await contextFactory.CreateDbContextAsync();
        var query = BuildSelectQuery(ctx, parameters, false, includes);
        var entities = await query.AsNoTracking().ToListAsync();
        return entities;
    }

    public virtual async Task<int> CountAsync()
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Set<T>().CountAsync();
    }

    private int GetPageCount(int totalCount, int countPerPage)
    {
        return countPerPage > 0
            ? (totalCount + countPerPage - 1) / countPerPage
            : 1;
    }

    /// <summary>
    /// Gets pagination information asynchronously.
    /// </summary>
    public virtual async Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        await using var ctx = await contextFactory.CreateDbContextAsync();
        int countPerPage = parameters.RowCount ?? 0;

        // явно без includes — для COUNT вони не потрібні
        var query = this.BuildSelectQuery(ctx, parameters, withoutPagination: true, includes: Array.Empty<string>());
        var totalCount = await query.CountAsync();

        return new PaginationInfo
        {
            TotalCount = totalCount,
            TotalPages = this.GetPageCount(totalCount, countPerPage),
        };
    }

    public virtual async Task<T?> FindAsync(int id, string[]? includes = null)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var includesToApply = includes ?? [];

        if (includesToApply.Length == 0)
        {
            var entity = await ctx.Set<T>().FindAsync(id);
            if (entity != null)
                ctx.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        IQueryable<T> query = ApplyIncludes(ctx.Set<T>(), includesToApply);
        return await query.AsNoTracking().Where("Id == @0", id).FirstOrDefaultAsync();
    }

    public virtual async Task<T?> FindAsync(params object[] keyValues)
    {
        if (keyValues == null || keyValues.Length == 0)
            return null;

        // одиночний int-ключ -> перегрузка з підтримкою includes
        if (keyValues.Length == 1 && keyValues[0] is int id)
            return await FindAsync(id);

        // складені ключі: includes по рядкових шляхах тут не застосовуються
        await using var ctx = await contextFactory.CreateDbContextAsync();
        var result = await ctx.Set<T>().FindAsync(keyValues);
        if (result != null)
            ctx.Entry(result).State = EntityState.Detached;
        return result;
    }

    public virtual async Task<List<T>> FindManyAsync(IEnumerable<int> ids, string[]? includes = null)
    {
        if (ids == null || !ids.Any())
            return new List<T>();

        var idList = ids.ToList();
        await using var ctx = await contextFactory.CreateDbContextAsync();
        var query = ApplyIncludes(ctx.Set<T>(), includes);

        return await query.AsNoTracking().Where("Id in @0", idList).ToListAsync();
    }

    public virtual async Task<List<T>> FindManyAsync(params int[] ids)
    {
        if (ids == null || ids.Length == 0)
            return new List<T>();

        return await FindManyAsync((IEnumerable<int>)ids);
    }

    /// <summary>
    /// Adds new entity.
    /// </summary>
    public virtual async Task<T> AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var ctx = await contextFactory.CreateDbContextAsync();
        ctx.Set<T>().Add(entity);
        await ctx.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await using var ctx = await contextFactory.CreateDbContextAsync();
        ctx.Set<T>().AddRange(entities);
        await ctx.SaveChangesAsync();
        return entities;
    }

    /// <summary>
    /// Updates existing entity.
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var ctx = await contextFactory.CreateDbContextAsync();
        ctx.Set<T>().Update(entity);
        int result = await ctx.SaveChangesAsync();

        if (result == 0)
        {
            throw new InvalidOperationException(
                $"Entity of type {typeof(T).Name} was not updated. It may not exist or has not changed.");
        }

        return entity;
    }

    public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await using var ctx = await contextFactory.CreateDbContextAsync();
        ctx.Set<T>().UpdateRange(entities);
        await ctx.SaveChangesAsync();
        return entities;
    }

    /// <summary>
    /// Deletes entity by primary key(s).
    /// </summary>
    public virtual async Task<bool> RemoveAsync(params object[] keyValues)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();

        var entity = await ctx.Set<T>().FindAsync(keyValues);
        if (entity == null)
        {
            throw new InvalidOperationException(
                $"Entity of type {typeof(T).Name} with keys {string.Join(", ", keyValues)} was not found.");
        }

        ctx.Set<T>().Remove(entity);
        int res = await ctx.SaveChangesAsync();
        return res > 0;
    }

    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<object[]> keyValuesList)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync();
        var entities = new List<T>();

        foreach (var keyValues in keyValuesList)
        {
            var entity = await ctx.Set<T>().FindAsync(keyValues);
            if (entity == null)
            {
                throw new InvalidOperationException(
                    $"Entity of type {typeof(T).Name} with keys {string.Join(", ", keyValues)} was not found.");
            }
            entities.Add(entity);
        }

        ctx.Set<T>().RemoveRange(entities);
        int res = await ctx.SaveChangesAsync();
        return res > 0;
    }

    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null || !entities.Any())
            return false;

        await using var ctx = await contextFactory.CreateDbContextAsync();
        ctx.Set<T>().RemoveRange(entities);
        int res = await ctx.SaveChangesAsync();
        return res > 0;
    }

    /// <summary>
    /// Retrieves an entity by ID and ensures it exists.
    /// </summary>
    protected virtual async Task<T> GetByIdAndEnsureExistsAsync(params object[] keyValues)
    {
        var entity = await this.FindAsync(keyValues);
        return entity ?? throw new InvalidOperationException(
            $"Entity of type {typeof(T).Name} with keys {string.Join(", ", keyValues)} was not found.");
    }

    /// <summary>
    /// Executes multiple operations in a single transaction.
    /// Use when you need atomicity across several SaveChanges calls within one repository.
    /// </summary>
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
    /// Executes multiple operations in a single transaction without a return value.
    /// </summary>
    public virtual async Task ExecuteInTransactionAsync(Func<TContext, Task> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteInTransactionAsync<object?>(async ctx =>
        {
            await operation(ctx);
            return null;
        });
    }

    public void ConfigureUserIdField(string[] userIdPropertyPaths)
    {
        this.userIdPropertyPaths = userIdPropertyPaths;
    }

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
                throw new InvalidOperationException($"Помилка при розборі шляху '{path}': {ex.Message}", ex);
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
