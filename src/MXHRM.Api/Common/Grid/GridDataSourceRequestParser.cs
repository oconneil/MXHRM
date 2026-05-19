using Microsoft.AspNetCore.Http;
using MXHRM.Application.Common.Grid;

namespace MXHRM.Api.Common.Grid;

public static class GridDataSourceRequestParser
{
    public static GridDataSourceRequest FromQuery(IQueryCollection query)
    {
        var request = new GridDataSourceRequest
        {
            Skip = GetInt(query, "skip", 0),
            Take = GetInt(query, "take", 20),
            Page = GetInt(query, "page", 1),
            PageSize = GetInt(query, "pageSize", GetInt(query, "take", 20)),
            FilterLogic = GetString(query, "filter[logic]", "and")
        };

        ParseSorts(query, request);
        ParseFilters(query, request);

        return request;
    }

    private static void ParseSorts(IQueryCollection query, GridDataSourceRequest request)
    {
        for (var index = 0; ; index++)
        {
            var field = GetString(query, $"sort[{index}][field]", string.Empty);

            if (string.IsNullOrWhiteSpace(field))
            {
                break;
            }

            request.Sorts.Add(new GridSortDescriptor
            {
                Field = field,
                Dir = GetString(query, $"sort[{index}][dir]", "asc")
            });
        }
    }

    private static void ParseFilters(IQueryCollection query, GridDataSourceRequest request)
    {
        for (var index = 0; ; index++)
        {
            var field = GetString(query, $"filter[filters][{index}][field]", string.Empty);

            if (string.IsNullOrWhiteSpace(field))
            {
                break;
            }

            request.Filters.Add(new GridFilterDescriptor
            {
                Field = field,
                Operator = GetString(query, $"filter[filters][{index}][operator]", "contains"),
                Value = GetOptionalString(query, $"filter[filters][{index}][value]")
            });
        }
    }

    private static int GetInt(IQueryCollection query, string key, int defaultValue)
    {
        if (!query.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        return int.TryParse(value.ToString(), out var parsedValue)
            ? parsedValue
            : defaultValue;
    }

    private static string GetString(IQueryCollection query, string key, string defaultValue)
    {
        if (!query.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        var text = value.ToString();

        return string.IsNullOrWhiteSpace(text)
            ? defaultValue
            : text;
    }

    private static string? GetOptionalString(IQueryCollection query, string key)
    {
        if (!query.TryGetValue(key, out var value))
        {
            return null;
        }

        var text = value.ToString();

        return string.IsNullOrWhiteSpace(text)
            ? null
            : text;
    }
}