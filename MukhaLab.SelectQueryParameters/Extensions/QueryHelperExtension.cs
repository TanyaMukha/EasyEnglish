using System.Linq.Expressions;
using MukhaLab.SelectQueryParameters.Models;

namespace MukhaLab.SelectQueryParameters.Extensions;

public static class QueryHelperExtensions
{
    #region ApplyQueryParameters

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

    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, List<FilterParameter> filters)
    {
        foreach (var filter in filters.Where(f => !string.IsNullOrWhiteSpace(f.Field)))
            query = query.ApplyFilter(filter);

        return query;
    }

    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, FilterParameter filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression expr = BuildFilterExpression(parameter, filter);

        if (expr == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(expr, parameter);
        return query.Where(lambda);
    }

    private static Expression BuildFilterExpression(ParameterExpression parameter, FilterParameter filter)
    {
        Expression propertyExpr = GetPropertyExpression(parameter, filter.Field);

        if (propertyExpr == null)
            return null!;

        // Підтримка collection через Any()
        if (propertyExpr.Type == typeof(bool) && filter.Operation != FilterOperation.Equal)
            return propertyExpr;

        var value = ConvertFilterValue(filter.Value, filter.DataType);
        var from = ConvertFilterValue(filter.From, filter.DataType);
        var to = ConvertFilterValue(filter.To, filter.DataType);

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

    private static Expression GetPropertyExpression(Expression parameter, string propertyPath)
    {
        Expression expr = parameter;
        var parts = propertyPath.Split('.');

        foreach (var part in parts)
        {
            if (part.Contains("[") && part.Contains("]"))
            {
                // Колекція
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

    private static object ConvertFilterValue(object value, FilterDataType dataType)
    {
        if (value == null)
            return null!;

        string str = value.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(str))
            return null!;

        return dataType switch
        {
            //FilterDataType.String => str,
            //FilterDataType.Integer => int.Parse(str),
            //FilterDataType.Decimal => decimal.Parse(str),
            //FilterDataType.DateTime => DateTime.Parse(str),
            //FilterDataType.Date => DateTime.Parse(str).Date,
            //FilterDataType.Boolean => bool.Parse(str),
            //FilterDataType.Guid => Guid.Parse(str),
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

    private static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string field, SortDirection direction)
    {
        return direction == SortDirection.Desc ? query.ThenByDescending(field) : query.ThenBy(field);
    }

    private static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "OrderBy");
    }

    private static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "OrderByDescending");
    }

    private static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "ThenBy");
    }

    private static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string propertyName)
    {
        return ApplyOrder(query, propertyName, "ThenByDescending");
    }

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

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    #endregion
}
