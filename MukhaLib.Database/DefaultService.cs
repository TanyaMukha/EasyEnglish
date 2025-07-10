using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MukhaLib.SelectQueryParameters.Extensions;
using MukhaLib.SelectQueryParameters.Models;

namespace MukhaLib.Database;

/// <summary>
/// Base implementation of a service providing CRUD operations with optional lazy loading of related entities.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public class DefaultService<T> : IDefaultService<T>
    where T : AbstractEntity
{
    protected readonly DbContext context;
    protected readonly DbSet<T> dbSet;
    protected readonly IMapper mapper;
    protected readonly IUserContext userContext;
    protected string[] includes = Array.Empty<string>();
    protected string[] userIdPropertyPaths = Array.Empty<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultService{T}"/> class.
    /// </summary>
    /// <param name="mapper">The AutoMapper instance for object mapping.</param>
    /// <param name="context">The database context.</param>
    public DefaultService(IMapper mapper, DbContext context, IUserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Set<T>());

        this.context = context;
        this.dbSet = context.Set<T>();
        this.mapper = mapper;
        this.userContext = userContext;
    }

    /// <summary>
    /// Retrieves all entities, including default related entities if specified.
    /// </summary>
    public virtual IEnumerable<T> GetAll(GetRequestParameters parameters)
    {
        IQueryable<T> query = this.dbSet;
        query = this.IncludeUserIdFilter(query);
        query = this.IncludeProperties(query);
        query = query.ApplyQueryParameters(parameters);
        return query.ToList();
    }

    public PaginationInfo GetPaginationInfo(GetRequestParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        IQueryable<T> query = this.dbSet;

        int? countPerPage = parameters.RowCount;
        parameters.PageNumber = null;
        parameters.RowCount = null;

        query = this.IncludeUserIdFilter(query);
        query = this.IncludeProperties(query);
        query = query.ApplyQueryParameters(parameters);

        PaginationInfo paginationInfo = new()
        {
            TotalCount = query.Count(),
            TotalPages = countPerPage.HasValue && countPerPage.Value > 0
                ? (query.Count() + countPerPage.Value - 1) / countPerPage.Value
                : 1,
        };

        return paginationInfo;
    }

    /// <summary>
    /// Retrieves an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    public virtual T? GetById(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        IQueryable<T> query = this.dbSet;
        query = this.IncludeProperties(query);
        query = this.IncludeUserIdFilter(query);
        return query.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    public virtual void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _ = this.dbSet.Add(entity);
        _ = this.context.SaveChanges();
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity with updated data.</param>
    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.Id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(entity));
        }

        var existingEntity = this.dbSet.Find(entity.Id);

        ArgumentNullException.ThrowIfNull(existingEntity);

        _ = this.mapper.Map(entity, existingEntity);
        _ = this.dbSet.Update(existingEntity);
        _ = this.context.SaveChanges();
    }

    /// <summary>
    /// Deletes an entity from the database.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public virtual void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = this.dbSet.Remove(entity);
        _ = this.context.SaveChanges();
    }

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    public virtual void DeleteById(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var entity = this.GetByIdAndEnsureExists(id);
        this.Delete(entity);
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
    /// Retrieves an entity by ID and ensures it exists.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The found entity.</returns>
    protected virtual T GetByIdAndEnsureExists(int id)
    {
        var entity = this.GetById(id);
        return entity ?? throw new InvalidOperationException($"Entity of type {typeof(T).Name} with ID {id} was not found.");
    }

    /// <summary>
    /// Includes related entities using string-based paths (avoids duplicates).
    /// </summary>
    /// <param name="query">The base query.</param>
    /// <param name="additionalIncludes">Additional include paths.</param>
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
        var userIdConst = Expression.Constant(this.userContext.GetCurrentUserId());
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
                        // it's a collection
                        var elementType = prop.PropertyType.GetGenericArguments().FirstOrDefault();
                        if (elementType == null)
                        {
                            break;
                        }

                        var itemParam = Expression.Parameter(elementType, "e");
                        var innerProp = parts[i + 1]; // skip to next segment
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

                // fallback to direct comparison if it's not a collection
                innerExpression ??= Expression.Equal(current, userIdConst);

                combined = combined == null ? innerExpression : Expression.OrElse(combined, innerExpression);
            }
            catch (Exception ex)
            {
                // другие исключения выбрасываем, чтобы не скрывать настоящие ошибки
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
