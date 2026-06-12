using System.Linq.Expressions;
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
    protected readonly IUserContext? userContext;
    protected string[] userIdPropertyPaths = Array.Empty<string>();
    protected bool enableUserFiltering = false;

    public BaseRepository(
        IDbContextFactory<TContext> contextFactory,
        IUserContext? userContext = null)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);

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

    /// <summary>
    /// Базовий select-запит з user-фільтром та includes.
    /// Спадкоємці використовують його як стартову точку для власних LINQ-запитів.
    /// </summary>
    protected IQueryable<T> BuildSelectQuery(TContext ctx, string[]? includes = null)
    {
        IQueryable<T> query = ctx.Set<T>();

        query = this.enableUserFiltering ? this.IncludeUserIdFilter(query) : query;
        query = ApplyIncludes(query, includes);

        return query;
    }

    /// <summary>
    /// Select-запит з динамічними параметрами (фільтри, сортування, пагінація).
    /// Зарезервовано для майбутніх динамічних фільтрів.
    /// </summary>
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

    /// <summary>
    /// Retrieves all entities as list asynchronously.
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAsync(string[]? includes = null, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = BuildSelectQuery(ctx, includes);
        var entities = await query.AsNoTracking().ToListAsync(cancellationToken);
        return entities;
    }

    /// <summary>
    /// Retrieves entities by dynamic query parameters.
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAsync(QueryParameters parameters, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = BuildSelectQuery(ctx, parameters, false, includes);
        var entities = await query.AsNoTracking().ToListAsync(cancellationToken);
        return entities;
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);

        // той самий user-фільтр, що й у вибірках — кількість має збігатися з даними
        IQueryable<T> query = ctx.Set<T>();
        query = this.enableUserFiltering ? this.IncludeUserIdFilter(query) : query;

        return await query.CountAsync(cancellationToken);
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
    public virtual async Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        int countPerPage = parameters.RowCount ?? 0;

        // явно без includes — для COUNT вони не потрібні
        var query = this.BuildSelectQuery(ctx, parameters, withoutPagination: true, includes: Array.Empty<string>());
        var totalCount = await query.CountAsync(cancellationToken);

        return new PaginationInfo
        {
            TotalCount = totalCount,
            TotalPages = this.GetPageCount(totalCount, countPerPage),
        };
    }

    public virtual async Task<T?> FindAsync(int id, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);

        var includesToApply = includes ?? [];

        if (includesToApply.Length == 0)
        {
            var entity = await ctx.Set<T>().FindAsync([id], cancellationToken);
            if (entity != null)
                ctx.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        IQueryable<T> query = ApplyIncludes(ctx.Set<T>(), includesToApply);
        return await query.AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);
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

    public virtual async Task<List<T>> FindManyAsync(IEnumerable<int> ids, string[]? includes = null, CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any())
            return new List<T>();

        var idList = ids.ToList();
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = ApplyIncludes(ctx.Set<T>(), includes);

        return await query.AsNoTracking()
            .Where(e => idList.Contains(EF.Property<int>(e, "Id")))
            .ToListAsync(cancellationToken);
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
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        ctx.Set<T>().Add(entity);
        await ctx.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        ctx.Set<T>().AddRange(entities);
        await ctx.SaveChangesAsync(cancellationToken);
        return entities;
    }

    /// <summary>
    /// Updates existing entity.
    /// Увага: Update() позначає всі поля зміненими — у базу пишеться повний стан entity.
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        ctx.Set<T>().Update(entity);
        int result = await ctx.SaveChangesAsync(cancellationToken);

        if (result == 0)
        {
            throw new InvalidOperationException(
                $"Entity of type {typeof(T).Name} was not updated — no rows were affected (the record may not exist).");
        }

        return entity;
    }

    public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        ctx.Set<T>().UpdateRange(entities);
        await ctx.SaveChangesAsync(cancellationToken);
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

    /// <summary>
    /// Видаляє записи за int-ключами одним батчем: один SELECT і один SaveChanges.
    /// </summary>
    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids?.Distinct().ToList() ?? [];
        if (idList.Count == 0)
            return false;

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);

        var entities = await ctx.Set<T>()
            .Where(e => idList.Contains(EF.Property<int>(e, "Id")))
            .ToListAsync(cancellationToken);

        if (entities.Count != idList.Count)
        {
            throw new InvalidOperationException(
                $"Entities of type {typeof(T).Name} were not all found: expected {idList.Count}, found {entities.Count}.");
        }

        ctx.Set<T>().RemoveRange(entities);
        int res = await ctx.SaveChangesAsync(cancellationToken);
        return res > 0;
    }

    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<object[]> keyValuesList, CancellationToken cancellationToken = default)
    {
        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entities = new List<T>();

        foreach (var keyValues in keyValuesList)
        {
            var entity = await ctx.Set<T>().FindAsync(keyValues, cancellationToken);
            if (entity == null)
            {
                throw new InvalidOperationException(
                    $"Entity of type {typeof(T).Name} with keys {string.Join(", ", keyValues)} was not found.");
            }
            entities.Add(entity);
        }

        ctx.Set<T>().RemoveRange(entities);
        int res = await ctx.SaveChangesAsync(cancellationToken);
        return res > 0;
    }

    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null || !entities.Any())
            return false;

        await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        ctx.Set<T>().RemoveRange(entities);
        int res = await ctx.SaveChangesAsync(cancellationToken);
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
