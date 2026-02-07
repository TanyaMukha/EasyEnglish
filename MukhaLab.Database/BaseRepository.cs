using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MukhaLab.SelectQueryParameters.Extensions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.Database;

/// <summary>
/// Base implementation of a service providing CRUD operations with optional lazy loading of related entities.
/// Thread-safe when using IDbContextFactory.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
/// <typeparam name="TContext">The type of the DbContext.</typeparam>
public abstract class BaseRepository<T, TContext> : IBaseRepository<T>
    where T : class
    where TContext : DbContext
{
    protected readonly TContext? context;
    protected readonly IDbContextFactory<TContext>? contextFactory;
    protected readonly DbSet<T>? dbSet;
    protected readonly IMapper mapper;
    protected readonly IUserContext? userContext;
    protected string[] includes = Array.Empty<string>();
    protected string[] userIdPropertyPaths = Array.Empty<string>();
    protected bool enableUserFiltering = false;

    private readonly bool useFactory;

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
        this.useFactory = true;
        this.context = null;
        this.dbSet = null;
    }

    // ✅ Конструктор зі звичайним DbContext (для існуючого коду)
    public BaseRepository(IMapper mapper, TContext context, IUserContext userContext)
        : this(mapper, context, userContext, true)
    {
    }

    public BaseRepository(IMapper mapper, TContext context)
        : this(mapper, context, null, false)
    {
    }

    private BaseRepository(IMapper mapper, TContext context, IUserContext? userContext, bool enableUserFiltering)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Set<T>());

        this.context = context;
        this.dbSet = context.Set<T>();
        this.mapper = mapper;
        this.userContext = userContext;
        this.enableUserFiltering = enableUserFiltering;
        this.useFactory = false;
        this.contextFactory = null;
    }

    /// <summary>
    /// Gets DbContext - creates new if using Factory, returns existing if using Scoped.
    /// IMPORTANT: When using Factory, caller must dispose the context!
    /// </summary>
    protected async Task<(TContext context, bool shouldDispose)> GetContextAsync()
    {
        if (useFactory)
        {
            var ctx = await contextFactory!.CreateDbContextAsync();
            return (ctx, shouldDispose: true);
        }

        return (context!, shouldDispose: false);
    }

    /// <summary>
    /// Gets DbSet from context.
    /// </summary>
    protected DbSet<T> GetDbSet(TContext ctx)
    {
        return useFactory ? ctx.Set<T>() : dbSet!;
    }

    private IQueryable<T> BuildSelectQuery(
        TContext ctx,
        QueryParameters parameters,
        bool withoutPagination = false,
        bool disabledIncludes = false)
    {
        var set = GetDbSet(ctx);
        IQueryable<T> query = set;

        var queryParameters = withoutPagination ? new QueryParameters
        {
            Filters = parameters.Filters,
            Sort = parameters.Sort,
            PageNumber = null,
            RowCount = null
        } : parameters;

        query = this.enableUserFiltering ? this.IncludeUserIdFilter(query) : query;
        if (!disabledIncludes)
        {
            query = this.IncludeProperties(query);
        }
        query = query.ApplyQueryParameters(queryParameters);

        return query;
    }

    /// <summary>
    /// Retrieves all entities as queryable, including default related entities if specified.
    /// </summary>
    public virtual IQueryable<T> Get(QueryParameters parameters, bool disabledIncludes = false)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (useFactory)
        {
            throw new InvalidOperationException(
                "Get(QueryParameters) cannot be used with Factory pattern. Use GetAsync instead.");
        }

        return this.BuildSelectQuery(context!, parameters, false, disabledIncludes);
    }

    /// <summary>
    /// Retrieves all entities as list asynchronously.
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAsync(QueryParameters parameters, bool disabledIncludes = false)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var query = this.BuildSelectQuery(ctx, parameters, false, disabledIncludes);
            return await query.AsNoTracking().ToListAsync();
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    public virtual async Task<int> CountAsync()
    {
        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);
            return await set.CountAsync();
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    private int GetPageCount(int totalCount, int countPerPage)
    {
        return countPerPage > 0
                ? (totalCount + countPerPage - 1) / countPerPage
                : 1;
    }

    /// <summary>
    /// Gets pagination information synchronously.
    /// </summary>
    public virtual PaginationInfo GetPaginationInfo(QueryParameters parameters, bool disabledIncludes = false)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (useFactory)
        {
            throw new InvalidOperationException(
                "GetPaginationInfo(QueryParameters) cannot be used with Factory pattern. Use GetPaginationInfoAsync instead.");
        }

        int countPerPage = parameters.RowCount ?? 0;
        var query = this.BuildSelectQuery(context!, parameters, withoutPagination: true, disabledIncludes: disabledIncludes);
        var totalCount = query.Count();

        return new PaginationInfo
        {
            TotalCount = totalCount,
            TotalPages = this.GetPageCount(totalCount, countPerPage),
        };
    }

    /// <summary>
    /// Gets pagination information asynchronously.
    /// </summary>
    public virtual async Task<PaginationInfo> GetPaginationInfoAsync(QueryParameters parameters, bool disabledIncludes = false)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            int countPerPage = parameters.RowCount ?? 0;
            var query = this.BuildSelectQuery(ctx, parameters, withoutPagination: true, disabledIncludes: disabledIncludes);
            var totalCount = await query.CountAsync();

            return new PaginationInfo
            {
                TotalCount = totalCount,
                TotalPages = this.GetPageCount(totalCount, countPerPage),
            };
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Finds entity by primary key(s).
    /// </summary>
    public virtual async Task<T?> FindAsync(params object[] keyValues)
    {
        if (keyValues == null || keyValues.Length == 0)
        {
            return null;
        }

        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);

            // Якщо немає налаштованих включень, використовуємо стандартний FindAsync
            if (this.includes == null || this.includes.Length == 0)
            {
                return await set.FindAsync(keyValues);
            }

            // Для простоти припускаємо, що primary key - це Id
            if (keyValues.Length == 1 && keyValues[0] is int id)
            {
                IQueryable<T> query = set;
                query = this.IncludeProperties(query);
                return await query.Where("Id == @0", id).FirstOrDefaultAsync();
            }

            return await set.FindAsync(keyValues);
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Finds entities by multiple primary keys.
    /// </summary>
    public virtual async Task<List<T>> FindManyAsync(params int[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return new List<T>();
        }

        return await FindManyAsync((IEnumerable<int>)ids);
    }

    /// <summary>
    /// Finds entities by multiple primary keys.
    /// </summary>
    public virtual async Task<List<T>> FindManyAsync(IEnumerable<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return new List<T>();
        }

        var idList = ids.ToList(); // Materialize для уникнення multiple enumeration
        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);
            IQueryable<T> query = set;

            if (this.includes != null && this.includes.Length > 0)
            {
                query = this.IncludeProperties(query);
            }

            return await query.Where("Id in @0", idList).ToListAsync();
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Adds new entity.
    /// </summary>
    public virtual async Task<T> AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);
            set.Add(entity);
            await ctx.SaveChangesAsync();
            return entity;
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);
            set.AddRange(entities);
            await ctx.SaveChangesAsync();
            return entities;
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Updates existing entity.
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);
            set.Update(entity);
            int result = await ctx.SaveChangesAsync();

            if (result == 0)
            {
                throw new InvalidOperationException($"Entity of type {typeof(T).Name} was not updated. It may not exist or has not changed.");
            }

            return entity;
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);
            set.UpdateRange(entities);
            int result = await ctx.SaveChangesAsync();

            //if (result == 0)
            //{
            //    throw new InvalidOperationException($"Entity of type {typeof(T).Name} was not updated. It may not exist or has not changed.");
            //}

            return entities;
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Deletes entity by primary key(s).
    /// </summary>
    public virtual async Task<bool> RemoveAsync(params object[] keyValues)
    {
        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var entity = await this.GetByIdAndEnsureExistsAsync(keyValues);

            var set = GetDbSet(ctx);
            set.Remove(entity);
            int res = await ctx.SaveChangesAsync();
            return res > 0;
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<object[]> keyValuesList)
    {
        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var entities = new List<T>();

            foreach (var keyValues in keyValuesList)
            {
                var entity = await this.GetByIdAndEnsureExistsAsync(keyValues);
                entities.Add(entity);
            }

            var set = GetDbSet(ctx);
            set.RemoveRange(entities);
            int res = await ctx.SaveChangesAsync();
            return res > 0;
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    public virtual async Task<bool> RemoveRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null || !entities.Any())
            return false;

        var (ctx, shouldDispose) = await GetContextAsync();
        try
        {
            var set = GetDbSet(ctx);
            set.RemoveRange(entities);
            int res = await ctx.SaveChangesAsync();
            return res > 0;
        }
        finally
        {
            if (shouldDispose)
            {
                await ctx.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Retrieves an entity by ID and ensures it exists.
    /// </summary>
    protected virtual async Task<T> GetByIdAndEnsureExistsAsync(params object[] keyValues)
    {
        var entity = await this.FindAsync(keyValues);
        return entity ?? throw new InvalidOperationException($"Entity of type {typeof(T).Name} with keys {keyValues} was not found.");
    }

    public void ConfigureIncludes(string[] includes)
    {
        this.includes = includes.Distinct().ToArray() ?? Array.Empty<string>();
    }

    public void ConfigureUserIdField(string[] userIdPropertyPaths)
    {
        this.userIdPropertyPaths = userIdPropertyPaths;
    }

    /// <summary>
    /// Includes related entities using string-based paths (avoids duplicates).
    /// </summary>
    protected virtual IQueryable<T> IncludeProperties(IQueryable<T> query, params string[] additionalIncludes)
    {
        var allIncludes = this.includes
            .Concat(additionalIncludes ?? Array.Empty<string>())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct();

        foreach (var include in allIncludes)
        {
            query = query.Include(include);
        }

        return query;
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
                throw new InvalidOperationException($"Ошибка при разборе пути '{path}': {ex.Message}", ex);
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

//3. Приклад використання в репозиторії
//Варіант А: Продовжуйте використовувати Scoped(БЕЗ ЗМІН) :
//csharppublic class CourseRepository : BaseRepository<CourseEntity>, ICourseRepository
//{
//    // ✅ Працює як раніше - використовує Scoped DbContext
//    public CourseRepository(
//        IMapper mapper,
//        EasyEnglishDbContext context,
//        IUserContext userContext)
//        : base(mapper, context, userContext)
//    {
//        ConfigureIncludes(new[] { "Units" });
//    }
//}
//Варіант Б: Використовуйте Factory(НОВЕ):
//csharppublic class CourseRepository : BaseRepository<CourseEntity>, ICourseRepository
//{
//    // ✅ Новий конструктор з Factory - потокобезпечний!
//    public CourseRepository(
//        IMapper mapper,
//        IDbContextFactory<EasyEnglishDbContext> contextFactory,
//        IUserContext? userContext = null)
//        : base(mapper, contextFactory, userContext)
//    {
//        ConfigureIncludes(new[] { "Units" });
//    }
//}