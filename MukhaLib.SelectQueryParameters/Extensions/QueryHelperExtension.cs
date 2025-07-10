using System.Linq.Expressions;
using System.Text.Json;
using MukhaLib.SelectQueryParameters.Models;
using System.Text.Json.Serialization;

namespace MukhaLib.SelectQueryParameters.Extensions;
/// <summary>
/// Extensions for working with LINQ queries and filtering parameters
/// </summary>
public static class QueryHelperExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Applies query parameters to IQueryable
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Source query</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Modified query</returns>
    public static IQueryable<T> ApplyQueryParameters<T>(this IQueryable<T> query, GetRequestParameters parameters)
    {
        if (parameters == null)
        {
            return query;
        }

        List<FilterParameter>? filters;
        if (!string.IsNullOrEmpty(parameters.Filters))
        {
            filters = JsonSerializer.Deserialize<List<FilterParameter>>(parameters.Filters, _jsonOptions);

            if (filters is not null)
            {
                query = query.ApplyFilters(filters);
            }
        }

        Dictionary<string, string?>? orderByValues;
        // Apply sorting
        if (!string.IsNullOrEmpty(parameters.Sort))
        {
            orderByValues = JsonSerializer.Deserialize<Dictionary<string, string?>>(parameters.Sort, _jsonOptions);

            query = query.ApplySorting(orderByValues!);
        }

        // Apply pagination
        if (parameters.PageNumber.HasValue && parameters.RowCount.HasValue)
        {
            query = query.ApplyPaging(parameters.PageNumber.Value, parameters.RowCount.Value);
        }

        return query;
    }

    /// <summary>
    /// Applies filters to a query
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Source query</param>
    /// <param name="filters">List of filters</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, List<FilterParameter> filters)
    {
        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Field))
            { continue; }

            query = query.ApplyFilter(filter);
        }

        return query;
    }

    /// <summary>
    /// Applies a single filter to a query
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Source query</param>
    /// <param name="filter">Filter</param>
    /// <returns>Filtered query</returns>
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, FilterParameter filter)
    {
        // Get parameter for lambda expression
        var parameter = Expression.Parameter(typeof(T), "x");

        // Get access to the property to filter by
        var property = GetPropertyExpression(parameter, filter.Field);

        if (property == null)
        {
            return query;
        }

        // Create an expression for filtering
        var filterExpression = CreateFilterExpression(property, filter);

        // Create lambda expression
        var lambda = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);

        // Apply filter through Where
        return query.Where(lambda);
    }

    /// <summary>
    /// Gets an Expression for property access, supports nested properties (e.g., "Task.Title")
    /// </summary>
    private static Expression GetPropertyExpression(ParameterExpression parameter, string propertyPath)
    {
        if (string.IsNullOrWhiteSpace(propertyPath))
        {
            return null!;
        }

        var parts = propertyPath.Split('.');
        Expression property = parameter;

        foreach (var part in parts)
        {
            // Check for indexer for collections
            if (part.Contains('[') && part.Contains(']'))
            {
                // Extract collection name
                var collectionName = part.Substring(0, part.IndexOf('['));

                // Add access to collection
                property = Expression.Property(property, collectionName);

                // Apply Any() for collection - will return true if at least one element satisfies the condition
                // This functionality requires additional implementation depending on specific needs
                // The basic idea is presented here
            }
            else
            {
                property = Expression.Property(property, part);
            }
        }

        return property;
    }

    /// <summary>
    /// Creates a filtering expression based on filter parameter
    /// </summary>
    private static Expression CreateFilterExpression(Expression property, FilterParameter filter)
    {
        // Convert filter value to the correct data type
        object convertedValue = ConvertFilterValue(filter.Value, filter.DataType);
        object convertedFrom = ConvertFilterValue(filter.From, filter.DataType);
        object convertedTo = ConvertFilterValue(filter.To, filter.DataType);

        // Create constant expression for filter value
        Expression valueExpression = convertedValue != null
            ? Expression.Constant(convertedValue, property.Type)
            : Expression.Constant(null);

        Expression fromExpression = convertedFrom != null
            ? Expression.Constant(convertedFrom, property.Type)
            : Expression.Constant(null);

        Expression toExpression = convertedTo != null
            ? Expression.Constant(convertedTo, property.Type)
            : Expression.Constant(null);

        switch (filter.Operation)
        {
            case FilterOperation.Equal:
                return Expression.Equal(property, valueExpression);

            case FilterOperation.NotEqual:
                return Expression.NotEqual(property, valueExpression);

            case FilterOperation.GreaterThan:
                return Expression.GreaterThan(property, valueExpression);

            case FilterOperation.GreaterThanOrEqual:
                return Expression.GreaterThanOrEqual(property, valueExpression);

            case FilterOperation.LessThan:
                return Expression.LessThan(property, valueExpression);

            case FilterOperation.LessThanOrEqual:
                return Expression.LessThanOrEqual(property, valueExpression);

            case FilterOperation.Contains:
                if (filter.DataType == FilterDataType.String)
                {
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    return Expression.Call(property, containsMethod!, valueExpression);
                }
                return null!;

            case FilterOperation.StartsWith:
                if (filter.DataType == FilterDataType.String)
                {
                    var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                    return Expression.Call(property, startsWithMethod!, valueExpression);
                }
                return null!;

            case FilterOperation.EndsWith:
                if (filter.DataType == FilterDataType.String)
                {
                    var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                    return Expression.Call(property, endsWithMethod!, valueExpression);
                }
                return null!;

            case FilterOperation.Between:
                Expression lowerBound = convertedFrom != null
                    ? Expression.GreaterThanOrEqual(property, fromExpression)
                    : Expression.Constant(true);

                Expression upperBound = convertedTo != null
                    ? Expression.LessThanOrEqual(property, toExpression)
                    : Expression.Constant(true);

                return Expression.AndAlso(lowerBound, upperBound);

            case FilterOperation.IsNull:
                return Expression.Equal(property, Expression.Constant(null));

            case FilterOperation.IsNotNull:
                return Expression.NotEqual(property, Expression.Constant(null));

            default:
                return null!;
        }
    }

    /// <summary>
    /// Converts filter value to the appropriate data type
    /// </summary>
    private static object ConvertFilterValue(object value, FilterDataType dataType)
    {
        if (value == null)
        {
            return null!;
        }

        string stringValue = value.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return null!;
        }

        try
        {
            switch (dataType)
            {
                case FilterDataType.String:
                    return stringValue;

                case FilterDataType.Integer:
                    return Convert.ToInt32(stringValue);

                case FilterDataType.Decimal:
                    return Convert.ToDecimal(stringValue);

                case FilterDataType.DateTime:
                    return Convert.ToDateTime(stringValue);

                case FilterDataType.Date:
                    return Convert.ToDateTime(stringValue).Date;

                case FilterDataType.Boolean:
                    return Convert.ToBoolean(stringValue);

                case FilterDataType.Guid:
                    return Guid.Parse(stringValue);

                default:
                    return stringValue;
            }
        }
        catch (FormatException)
        {
            return null!;
        }
        catch (OverflowException)
        {
            return null!;
        }
    }

    /// <summary>
    /// Applies sorting to a query
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Source query</param>
    /// <param name="sortExpression">Sort expression (e.g., "Name", "Name desc", "Date asc")</param>
    /// <returns>Sorted query</returns>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, Dictionary<string, string?> orderByValues)
    {
        bool isFirstSort = true;

        foreach (var orderBy in orderByValues)
        {
            if (string.IsNullOrEmpty(orderBy.Key))
            {
                continue;
            }

            // Apply sorting
            if (isFirstSort)
            {
                query = orderBy.Value!.Contains("desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(orderBy.Key)
                    : query.OrderBy(orderBy.Key);

                isFirstSort = false;
            }
            else
            {
                query = orderBy.Value!.Contains("desc", StringComparison.OrdinalIgnoreCase)
                    ? ((IOrderedQueryable<T>)query).ThenByDescending(orderBy.Key)
                    : ((IOrderedQueryable<T>)query).ThenBy(orderBy.Key);
            }
        }

        return query;
    }

    /// <summary>
    /// Applies OrderBy to a query using property name
    /// </summary>
    private static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "OrderBy");
    }

    /// <summary>
    /// Applies OrderByDescending to a query using property name
    /// </summary>
    private static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "OrderByDescending");
    }

    /// <summary>
    /// Applies ThenBy to a query using property name
    /// </summary>
    private static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "ThenBy");
    }

    /// <summary>
    /// Applies ThenByDescending to a query using property name
    /// </summary>
    private static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "ThenByDescending");
    }

    /// <summary>
    /// Applies sort method to a query
    /// </summary>
    private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> query, string propertyName, string methodName)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = GetPropertyExpression(parameter, propertyName);

        if (property == null)
        {
            return (IOrderedQueryable<T>)query;
        }

        var lambda = Expression.Lambda(property, parameter);

        var method = typeof(Queryable).GetMethods()
            .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
            .Single(m => m.GetParameters().Length == 2);

        var genericMethod = method.MakeGenericMethod(typeof(T), property.Type);

        return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { query, lambda })!;
    }

    /// <summary>
    /// Applies pagination to a query
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Source query</param>
    /// <param name="pageNumber">Page number (starting from 1)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Query with pagination</returns>
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}
