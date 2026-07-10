using System.Linq.Expressions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Extensions;

/// <summary>
/// Translates <see cref="QueryParameters"/> (filters, sorting, paging) into
/// <see cref="System.Linq.Expressions.Expression"/> trees and applies them to an
/// <see cref="IQueryable{T}"/>. Works against any LINQ provider, including EF Core, since it only
/// composes standard <see cref="Queryable"/> operators (<c>Where</c>, <c>OrderBy</c>, <c>Skip</c>,
/// <c>Take</c>, ...).
/// </summary>
public static class QueryHelperExtensions
{
    #region ApplyQueryParameters

    /// <summary>
    /// Applies filtering, then sorting, then paging from <paramref name="parameters"/> to
    /// <paramref name="query"/>, in that order. Each step is skipped when the corresponding data is
    /// absent (null <see cref="QueryParameters.Filters"/>/<see cref="QueryParameters.Sort"/>, or a
    /// missing <see cref="QueryParameters.PageNumber"/>/<see cref="QueryParameters.RowCount"/> pair).
    /// </summary>
    /// <typeparam name="T">Element type of the query.</typeparam>
    /// <param name="query">Source query.</param>
    /// <param name="parameters">Filtering, sorting, and paging instructions. A null value is a no-op.</param>
    /// <returns>The query with all applicable steps composed onto it.</returns>
    public static IQueryable<T> ApplyQueryParameters<T>(this IQueryable<T> query, QueryParameters parameters)
    {
        if (parameters?.Filters != null)
            query = query.ApplyFilters(parameters.Filters);

        if (parameters?.Sort != null)
            query = query.ApplySorting(parameters.Sort);

        if (parameters?.PageNumber.HasValue == true && parameters?.RowCount.HasValue == true)
            query = query.ApplyPaging(parameters.PageNumber.Value, parameters.RowCount.Value);

        return query;
    }

    #endregion

    #region Filters

    /// <summary>
    /// Applies every filter in <paramref name="filters"/> as a separate, AND-combined <c>Where</c>
    /// clause. Filters with a blank <see cref="FilterValue.Field"/> are skipped.
    /// </summary>
    /// <typeparam name="T">Element type of the query.</typeparam>
    /// <param name="query">Source query.</param>
    /// <param name="filters">Filters to apply.</param>
    /// <returns>The query with one <c>Where</c> clause added per valid filter.</returns>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, List<FilterParameter> filters)
    {
        foreach (var filter in filters.Where(f => !string.IsNullOrWhiteSpace(f.Field)))
            query = query.ApplyFilter(filter);

        return query;
    }

    /// <summary>
    /// Compiles a single <see cref="FilterParameter"/> into a <c>Where</c> predicate and applies it.
    /// </summary>
    /// <typeparam name="T">Element type of the query.</typeparam>
    /// <param name="query">Source query.</param>
    /// <param name="filter">Filter to apply.</param>
    /// <returns>
    /// The query with the filter's <c>Where</c> clause added, or the unmodified
    /// <paramref name="query"/> if the filter could not be translated into an expression.
    /// </returns>
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, FilterParameter filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression expr = BuildFilterExpression(parameter, filter);

        if (expr == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(expr, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Builds the boolean expression for one filter: resolves <see cref="FilterValue.Field"/> to a
    /// property/navigation expression, then combines it with the converted filter value(s)
    /// according to <see cref="FilterValue.Operation"/>.
    /// </summary>
    /// <remarks>
    /// <b>Collection paths</b> (e.g. <c>"Executors[Title]"</c>) resolve to a boolean
    /// <c>collection.Any(item =&gt; item.Property != null)</c> expression produced by
    /// <see cref="GetPropertyExpression"/>. When <see cref="FilterValue.Operation"/> is anything
    /// other than <see cref="FilterOperation.Equal"/>, that boolean expression is returned directly
    /// — it is an existence check only and does <b>not</b> compare against
    /// <see cref="FilterParameter.Value"/>. If <see cref="FilterOperation.Equal"/> is used on a
    /// collection path, execution falls through to the normal comparison below, which then compares
    /// that boolean result against <see cref="FilterParameter.Value"/> (so <see cref="FilterDataType.Boolean"/>
    /// must be used in that case).
    /// </remarks>
    private static Expression BuildFilterExpression(ParameterExpression parameter, FilterParameter filter)
    {
        Expression propertyExpr = GetPropertyExpression(parameter, filter.Field);

        if (propertyExpr == null)
            return null!;

        // Collection path (see GetPropertyExpression): resolved to an Any() existence check.
        // For every operation except Equal, return that boolean check as-is instead of trying
        // to compare it against filter.Value.
        if (propertyExpr.Type == typeof(bool) && filter.Operation != FilterOperation.Equal)
            return propertyExpr;

        var value = ConvertFilterValue(filter.Value, filter.DataType);
        var from = ConvertFilterValue(filter.From, filter.DataType);
        var to = ConvertFilterValue(filter.To, filter.DataType);

        // Expression.Constant(null) without an explicit type yields Type == typeof(object); this
        // is only assignment-compatible with reference types and Nullable<T> value types, so
        // IsNull/IsNotNull below throws for non-nullable value type properties.
        Expression valueExpr = value is not null ? Expression.Constant(value, propertyExpr.Type) : Expression.Constant(null);
        Expression fromExpr = from is not null ? Expression.Constant(from, propertyExpr.Type) : Expression.Constant(null);
        Expression toExpr = to is not null ? Expression.Constant(to, propertyExpr.Type) : Expression.Constant(null);

        return filter.Operation switch
        {
            FilterOperation.Equal => Expression.Equal(propertyExpr, valueExpr),
            FilterOperation.NotEqual => Expression.NotEqual(propertyExpr, valueExpr),
            FilterOperation.GreaterThan => Expression.GreaterThan(propertyExpr, valueExpr),
            FilterOperation.GreaterThanOrEqual => Expression.GreaterThanOrEqual(propertyExpr, valueExpr),
            FilterOperation.LessThan => Expression.LessThan(propertyExpr, valueExpr),
            FilterOperation.LessThanOrEqual => Expression.LessThanOrEqual(propertyExpr, valueExpr),
            FilterOperation.Contains => Expression.Call(propertyExpr, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, valueExpr),
            FilterOperation.StartsWith => Expression.Call(propertyExpr, typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!, valueExpr),
            FilterOperation.EndsWith => Expression.Call(propertyExpr, typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!, valueExpr),
            FilterOperation.Between => Expression.AndAlso(Expression.GreaterThanOrEqual(propertyExpr, fromExpr), Expression.LessThanOrEqual(propertyExpr, toExpr)),
            FilterOperation.IsNull => Expression.Equal(propertyExpr, Expression.Constant(null)),
            FilterOperation.IsNotNull => Expression.NotEqual(propertyExpr, Expression.Constant(null)),
            _ => null!
        };
    }

    /// <summary>
    /// Resolves a dotted/bracketed property path (as used by <see cref="FilterValue.Field"/> and
    /// <see cref="SortDescriptor.Field"/>) into an expression rooted at <paramref name="parameter"/>.
    /// </summary>
    /// <remarks>
    /// Each dot-separated segment is either:
    /// <list type="bullet">
    /// <item><description>A plain property name (e.g. <c>"Title"</c>), resolved with <see cref="Expression.Property(Expression, string)"/>.</description></item>
    /// <item><description>
    /// A collection segment <c>"Collection[Property]"</c> (e.g. <c>"Executors[Title]"</c>), resolved
    /// to <c>collection.Any(item =&gt; item.Property != null)</c> — a boolean existence check for a
    /// non-null nested property, <b>not</b> a value comparison. It ignores whatever filter value the
    /// caller supplied. See <see cref="BuildFilterExpression"/> for how this interacts with
    /// <see cref="FilterOperation"/>. Once a collection segment is resolved the result is boolean, so
    /// it cannot be the target of a further dot-separated segment.
    /// </description></item>
    /// </list>
    /// </remarks>
    /// <param name="parameter">Root expression (typically the lambda parameter for the entity type).</param>
    /// <param name="propertyPath">Dot/bracket path, e.g. <c>"Title"</c>, <c>"Task.Title"</c>, or <c>"Executors[Title]"</c>.</param>
    /// <returns>The resolved expression.</returns>
    private static Expression GetPropertyExpression(Expression parameter, string propertyPath)
    {
        Expression expr = parameter;
        var parts = propertyPath.Split('.');

        foreach (var part in parts)
        {
            if (part.Contains("[") && part.Contains("]"))
            {
                // Collection segment: "Collection[Property]" -> Collection.Any(i => i.Property != null)
                var collectionName = part.Substring(0, part.IndexOf('['));
                var propertyName = part.Substring(part.IndexOf('[') + 1, part.IndexOf(']') - part.IndexOf('[') - 1);

                var collectionProperty = Expression.Property(expr, collectionName);
                var itemType = collectionProperty.Type.GetGenericArguments()[0];

                var lambdaParam = Expression.Parameter(itemType, "i");
                var itemProperty = Expression.Property(lambdaParam, propertyName);

                var anyLambda = Expression.Lambda(itemProperty != null ? Expression.NotEqual(itemProperty, Expression.Constant(null)) : Expression.Constant(true), lambdaParam);

                var anyMethod = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(itemType);

                expr = Expression.Call(anyMethod, collectionProperty, anyLambda);
            }
            else
            {
                expr = Expression.Property(expr, part);
            }
        }

        return expr;
    }

    /// <summary>
    /// Converts a raw, boxed filter value to the CLR type indicated by <paramref name="dataType"/>.
    /// </summary>
    /// <remarks>
    /// The value is always round-tripped through <see cref="object.ToString"/> before conversion —
    /// even if it is already the target type — so a boxed <see cref="int"/> and its string
    /// representation are handled identically. A null value or a blank string both convert to
    /// <c>null</c>.
    /// </remarks>
    /// <param name="value">Raw value, typically a string from a query string or a JSON payload.</param>
    /// <param name="dataType">Target CLR type.</param>
    /// <returns>The converted value, or <c>null</c> if <paramref name="value"/> is null/blank.</returns>
    private static object ConvertFilterValue(object value, FilterDataType dataType)
    {
        if (value == null)
            return null!;

        string str = value.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(str))
            return null!;

        return dataType switch
        {
            FilterDataType.String => str,
            FilterDataType.Integer => Convert.ToInt32(str),
            FilterDataType.Decimal => Convert.ToDecimal(str),
            FilterDataType.DateTime => Convert.ToDateTime(str),
            FilterDataType.Date => Convert.ToDateTime(str).Date,
            FilterDataType.Boolean => Convert.ToBoolean(str),
            FilterDataType.Guid => Guid.Parse(str),
            _ => str
        };
    }

    #endregion

    #region Sorting

    /// <summary>
    /// Applies every sort key in <paramref name="sortDescriptors"/> in list order: the first key
    /// becomes <c>OrderBy</c>/<c>OrderByDescending</c>, every subsequent key becomes
    /// <c>ThenBy</c>/<c>ThenByDescending</c>. Descriptors with a blank
    /// <see cref="SortDescriptor.Field"/> are skipped.
    /// </summary>
    /// <typeparam name="T">Element type of the query.</typeparam>
    /// <param name="query">Source query.</param>
    /// <param name="sortDescriptors">Sort keys to apply, in priority order.</param>
    /// <returns>The sorted query, or the original query unchanged if no valid sort key is present.</returns>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, List<SortDescriptor> sortDescriptors)
    {
        bool first = true;
        foreach (var sort in sortDescriptors.Where(s => !string.IsNullOrWhiteSpace(s.Field)))
        {
            query = first
                ? (sort.Direction == SortDirection.Desc ? query.OrderByDescending(sort.Field) : query.OrderBy(sort.Field))
                : ((IOrderedQueryable<T>)query).ThenBy(sort.Field, sort.Direction);

            first = false;
        }
        return query;
    }

    /// <summary>Applies <c>ThenBy</c> or <c>ThenByDescending</c> depending on <paramref name="direction"/>.</summary>
    private static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string field, SortDirection direction)
    {
        return direction == SortDirection.Desc ? query.ThenByDescending(field) : query.ThenBy(field);
    }

    /// <summary>String-keyed <c>Queryable.OrderBy</c>, resolving <paramref name="propertyName"/> via <see cref="GetPropertyExpression"/>.</summary>
    private static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "OrderBy");
    }

    /// <summary>String-keyed <c>Queryable.OrderByDescending</c>, resolving <paramref name="propertyName"/> via <see cref="GetPropertyExpression"/>.</summary>
    private static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "OrderByDescending");
    }

    /// <summary>String-keyed <c>Queryable.ThenBy</c>, resolving <paramref name="propertyName"/> via <see cref="GetPropertyExpression"/>.</summary>
    private static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "ThenBy");
    }

    /// <summary>String-keyed <c>Queryable.ThenByDescending</c>, resolving <paramref name="propertyName"/> via <see cref="GetPropertyExpression"/>.</summary>
    private static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "ThenByDescending");
    }

    /// <summary>
    /// Resolves <paramref name="propertyName"/> to an expression, then invokes the matching generic
    /// <see cref="Queryable"/> ordering method (<paramref name="methodName"/>) via reflection, since
    /// the resolved property type is only known at runtime.
    /// </summary>
    private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> query, string propertyName, string methodName)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = GetPropertyExpression(parameter, propertyName);

        var lambda = Expression.Lambda(property, parameter);
        var method = typeof(Queryable).GetMethods()
            .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
            .Single(m => m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type);

        return (IOrderedQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }

    #endregion

    #region Paging

    /// <summary>
    /// Applies one-based paging via <c>Skip</c>/<c>Take</c>. Out-of-range inputs are clamped rather
    /// than throwing: <paramref name="pageNumber"/> below 1 becomes 1, and
    /// <paramref name="pageSize"/> below 1 becomes 10.
    /// </summary>
    /// <typeparam name="T">Element type of the query.</typeparam>
    /// <param name="query">Source query. Should already be sorted for stable paging.</param>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum number of rows per page.</param>
    /// <returns>The query with <c>Skip</c>/<c>Take</c> applied.</returns>
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    #endregion
}
