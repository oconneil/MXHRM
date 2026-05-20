using Microsoft.AspNetCore.Http;
using MXHRM.Application.Common.Grid;

namespace MXHRM.Api.Common.Grid;

public static class GridDataSourceRequestParser
{
    public static GridDataSourceRequest FromQuery(IQueryCollection query)
    {
        var page = GetInt(query, "page", 1);
        var pageSize = GetInt(query, "pageSize", GetInt(query, "take", 20));
        var skip = query.ContainsKey("skip")
            ? GetInt(query, "skip", 0)
            : (page - 1) * pageSize;

        var request = new GridDataSourceRequest
        {
            Skip = skip,
            Take = GetInt(query, "take", pageSize),
            Page = page,
            PageSize = pageSize,
            FilterLogic = GetString(query, "filter[logic]", "and")
        };

        ParseSorts(query, request);
        ParseFilters(query, request);

        return request;
    }

    private static void ParseSorts(IQueryCollection query, GridDataSourceRequest request)
    {
        var mvcSort = GetOptionalString(query, "sort");

        if (!string.IsNullOrWhiteSpace(mvcSort))
        {
            ParseMvcSorts(mvcSort, request);
            return;
        }

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

    private static void ParseMvcSorts(string sortText, GridDataSourceRequest request)
    {
        foreach (var sort in sortText.Split('~', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = sort.LastIndexOf('-');

            if (separatorIndex <= 0)
            {
                request.Sorts.Add(new GridSortDescriptor
                {
                    Field = sort,
                    Dir = "asc"
                });

                continue;
            }

            request.Sorts.Add(new GridSortDescriptor
            {
                Field = sort[..separatorIndex],
                Dir = sort[(separatorIndex + 1)..]
            });
        }
    }

    private static void ParseFilters(IQueryCollection query, GridDataSourceRequest request)
    {
        var mvcFilter = GetOptionalString(query, "filter");

        if (!string.IsNullOrWhiteSpace(mvcFilter))
        {
            request.Filter = MvcFilterParser.Parse(mvcFilter);
            return;
        }

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

    private sealed class MvcFilterParser
    {
        private readonly IReadOnlyList<string> _tokens;
        private int _position;

        private MvcFilterParser(string filterText)
        {
            _tokens = Tokenize(filterText);
        }

        public static GridFilterDescriptor? Parse(string filterText)
        {
            var parser = new MvcFilterParser(filterText);
            return parser.ParseExpression();
        }

        private GridFilterDescriptor? ParseExpression()
        {
            var left = ParseTerm();

            while (TryPeek(out var token) && IsLogic(token))
            {
                var logic = Next().ToLowerInvariant();
                var right = ParseTerm();

                if (left is null)
                {
                    left = right;
                    continue;
                }

                if (right is null)
                {
                    continue;
                }

                left = Merge(logic, left, right);
            }

            return left;
        }

        private GridFilterDescriptor? ParseTerm()
        {
            if (!TryPeek(out var token))
            {
                return null;
            }

            if (token == "(")
            {
                Next();
                var expression = ParseExpression();

                if (TryPeek(out var closingToken) && closingToken == ")")
                {
                    Next();
                }

                return expression;
            }

            var field = Next();

            if (!TryPeek(out var operatorToken) ||
                operatorToken is "(" or ")" ||
                IsLogic(operatorToken))
            {
                return null;
            }

            var filterOperator = Next();

            if (!TryPeek(out var valueToken) ||
                valueToken is "(" or ")" ||
                IsLogic(valueToken))
            {
                return null;
            }

            return new GridFilterDescriptor
            {
                Field = field,
                Operator = filterOperator,
                Value = Unquote(Next())
            };
        }

        private static GridFilterDescriptor Merge(
            string logic,
            GridFilterDescriptor left,
            GridFilterDescriptor right)
        {
            var composite = new GridFilterDescriptor
            {
                Logic = logic
            };

            AddChild(composite, left, logic);
            AddChild(composite, right, logic);

            return composite;
        }

        private static void AddChild(
            GridFilterDescriptor composite,
            GridFilterDescriptor child,
            string logic)
        {
            if (child.IsComposite &&
                child.Logic.Equals(logic, StringComparison.OrdinalIgnoreCase))
            {
                composite.Filters.AddRange(child.Filters);
                return;
            }

            composite.Filters.Add(child);
        }

        private bool TryPeek(out string token)
        {
            if (_position >= _tokens.Count)
            {
                token = string.Empty;
                return false;
            }

            token = _tokens[_position];
            return true;
        }

        private string Next()
        {
            return _tokens[_position++];
        }

        private static bool IsLogic(string token)
        {
            return token.Equals("and", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("or", StringComparison.OrdinalIgnoreCase);
        }

        private static string Unquote(string value)
        {
            if (value.StartsWith("datetime'", StringComparison.OrdinalIgnoreCase) &&
                value.EndsWith('\''))
            {
                var dateTimeText = value[9..^1];
                var timeSeparatorIndex = dateTimeText.IndexOf('T');

                if (timeSeparatorIndex >= 0)
                {
                    dateTimeText = dateTimeText[..(timeSeparatorIndex + 1)] +
                        dateTimeText[(timeSeparatorIndex + 1)..].Replace('-', ':');
                }

                return dateTimeText;
            }

            if (value.Length >= 2 &&
                value.StartsWith('\'') &&
                value.EndsWith('\''))
            {
                return value[1..^1].Replace("''", "'", StringComparison.Ordinal);
            }

            return value;
        }

        private static IReadOnlyList<string> Tokenize(string filterText)
        {
            var tokens = new List<string>();
            var current = new System.Text.StringBuilder();
            var inQuote = false;

            for (var index = 0; index < filterText.Length; index++)
            {
                var character = filterText[index];

                if (character == '\'')
                {
                    current.Append(character);

                    if (inQuote &&
                        index + 1 < filterText.Length &&
                        filterText[index + 1] == '\'')
                    {
                        current.Append(filterText[++index]);
                        continue;
                    }

                    inQuote = !inQuote;
                    continue;
                }

                if (!inQuote && (character is '(' or ')' or '~'))
                {
                    AddToken(tokens, current);

                    if (character is '(' or ')')
                    {
                        tokens.Add(character.ToString());
                    }

                    continue;
                }

                current.Append(character);
            }

            AddToken(tokens, current);

            return tokens;
        }

        private static void AddToken(
            List<string> tokens,
            System.Text.StringBuilder current)
        {
            if (current.Length == 0)
            {
                return;
            }

            var token = current.ToString().Trim();
            current.Clear();

            if (!string.IsNullOrWhiteSpace(token))
            {
                tokens.Add(token);
            }
        }
    }
}
