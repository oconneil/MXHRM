using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MXHRM.Application.Common.Grid;

namespace MXHRM.Infrastructure.Common.Grid;

public static class GridQueryableExtensions
{
    public static async Task<GridDataSourceResult<T>> ToGridDataSourceResultAsync<T>(
        this IQueryable<T> query,
        GridDataSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var filteredQuery = ApplyFilters(query, request);

        var total = await filteredQuery.CountAsync(cancellationToken);

        var sortedQuery = ApplySorting(filteredQuery, request);

        var data = await sortedQuery
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new GridDataSourceResult<T>
        {
            Data = data,
            Total = total
        };
    }

    private static IQueryable<T> ApplyFilters<T>(
        IQueryable<T> query,
        GridDataSourceRequest request)
    {
        foreach (var filter in request.Filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Field) ||
                string.IsNullOrWhiteSpace(filter.Value))
            {
                continue;
            }

            query = ApplyFilter(query, filter);
        }

        return query;
    }

    private static IQueryable<T> ApplyFilter<T>(
        IQueryable<T> query,
        GridFilterDescriptor filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = BuildPropertyExpression(parameter, filter.Field);

        if (property is null)
        {
            return query;
        }

        var propertyType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;
        var convertedValue = ConvertFilterValue(filter.Value, propertyType);

        if (convertedValue is null)
        {
            return query;
        }

        var constant = Expression.Constant(convertedValue, property.Type);
        var expression = BuildFilterExpression(property, constant, filter.Operator);

        if (expression is null)
        {
            return query;
        }

        var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
        return query.Where(lambda);
    }

    private static Expression? BuildFilterExpression(
        Expression property,
        Expression constant,
        string filterOperator)
    {
        var normalizedOperator = filterOperator.ToLowerInvariant();

        if (property.Type == typeof(string))
        {
            return normalizedOperator switch
            {
                "contains" => Expression.Call(
                    property,
                    nameof(string.Contains),
                    Type.EmptyTypes,
                    constant),

                "doesnotcontain" => Expression.Not(Expression.Call(
                    property,
                    nameof(string.Contains),
                    Type.EmptyTypes,
                    constant)),

                "startswith" => Expression.Call(
                    property,
                    nameof(string.StartsWith),
                    Type.EmptyTypes,
                    constant),

                "endswith" => Expression.Call(
                    property,
                    nameof(string.EndsWith),
                    Type.EmptyTypes,
                    constant),

                "eq" => Expression.Equal(property, constant),

                "neq" => Expression.NotEqual(property, constant),

                _ => null
            };
        }

        return normalizedOperator switch
        {
            "eq" => Expression.Equal(property, constant),
            "neq" => Expression.NotEqual(property, constant),
            "gte" => Expression.GreaterThanOrEqual(property, constant),
            "gt" => Expression.GreaterThan(property, constant),
            "lte" => Expression.LessThanOrEqual(property, constant),
            "lt" => Expression.LessThan(property, constant),
            _ => null
        };
    }

    private static IQueryable<T> ApplySorting<T>(
        IQueryable<T> query,
        GridDataSourceRequest request)
    {
        if (request.Sorts.Count == 0)
        {
            return query;
        }

        IOrderedQueryable<T>? orderedQuery = null;

        for (var index = 0; index < request.Sorts.Count; index++)
        {
            var sort = request.Sorts[index];

            if (string.IsNullOrWhiteSpace(sort.Field))
            {
                continue;
            }

            orderedQuery = ApplySort(
                orderedQuery ?? query,
                sort.Field,
                sort.Dir,
                index > 0);
        }

        return orderedQuery ?? query;
    }

    private static IOrderedQueryable<T> ApplySort<T>(
        IQueryable<T> query,
        string field,
        string dir,
        bool thenBy)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = BuildPropertyExpression(parameter, field);

        if (property is null)
        {
            return query.OrderBy(_ => 0);
        }

        var lambda = Expression.Lambda(property, parameter);

        var methodName = (dir.Equals("desc", StringComparison.OrdinalIgnoreCase), thenBy) switch
        {
            (true, false) => nameof(Queryable.OrderByDescending),
            (false, false) => nameof(Queryable.OrderBy),
            (true, true) => nameof(Queryable.ThenByDescending),
            (false, true) => nameof(Queryable.ThenBy)
        };

        var method = typeof(Queryable)
            .GetMethods()
            .Single(m =>
                m.Name == methodName &&
                m.GetParameters().Length == 2);

        var genericMethod = method.MakeGenericMethod(typeof(T), property.Type);

        return (IOrderedQueryable<T>)genericMethod.Invoke(
            null,
            new object[] { query, lambda })!;
    }

    private static Expression? BuildPropertyExpression(
        Expression parameter,
        string field)
    {
        var property = typeof(object);

        Expression current = parameter;

        foreach (var member in field.Split('.'))
        {
            var propertyInfo = current.Type.GetProperty(member);

            if (propertyInfo is null)
            {
                propertyInfo = current.Type
                    .GetProperties()
                    .FirstOrDefault(p =>
                        p.Name.Equals(member, StringComparison.OrdinalIgnoreCase));
            }

            if (propertyInfo is null)
            {
                return null;
            }

            current = Expression.Property(current, propertyInfo);
        }

        return current;
    }

    private static object? ConvertFilterValue(string? value, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (targetType == typeof(string))
        {
            return value;
        }

        if (targetType == typeof(Guid))
        {
            return Guid.TryParse(value, out var guidValue) ? guidValue : null;
        }

        if (targetType == typeof(int))
        {
            return int.TryParse(value, out var intValue) ? intValue : null;
        }

        if (targetType == typeof(decimal))
        {
            return decimal.TryParse(value, out var decimalValue) ? decimalValue : null;
        }

        if (targetType == typeof(double))
        {
            return double.TryParse(value, out var doubleValue) ? doubleValue : null;
        }

        if (targetType == typeof(bool))
        {
            return bool.TryParse(value, out var boolValue) ? boolValue : null;
        }

        if (targetType == typeof(DateTime))
        {
            return DateTime.TryParse(value, out var dateTimeValue) ? dateTimeValue : null;
        }

        if (targetType.IsEnum)
        {
            return Enum.TryParse(targetType, value, true, out var enumValue)
                ? enumValue
                : null;
        }

        return Convert.ChangeType(value, targetType);
    }
}