using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using RefScout.Analyzer.Notes;

namespace RefScout.Analyzer.Filter;

public static class FilterParser
{
    public static Func<Assembly, bool> Parse(string filter)
    {
        _ = filter ?? throw new ArgumentNullException(nameof(filter));

        var query = new StringBuilder();
        var isNameQuery = true;
        var builder = PredicateBuilder.True<Assembly>();
        foreach (var part in new StringSplitEnumerator(filter))
        {
            if (AppendFilterToExpression(ref builder, part))
            {
                isNameQuery = false;
            }

            if (isNameQuery)
            {
                query
                    .Append(query.Length > 0 ? " " : string.Empty)
                    .Append(part);
            }
        }

        if (query.Length != 0)
        {
            builder = builder.And(x => CreateStringPredicate(query.ToString())(x.Name));
        }

        return builder.Compile();
    }

    private static bool AppendFilterToExpression(
        ref Expression<Func<Assembly, bool>> expression,
        ReadOnlySpan<char> part)
    {
        if (part.IndexOf(":") == -1)
        {
            return false;
        }

        var (negate, key, value) = ParseKeyValue(part);
        if (key.Length == 0 || value.Length == 0)
        {
            return false;
        }

        var filter = ExpressionFromKeyValue(key, value);
        if (filter == null)
        {
            return false;
        }

        expression = expression.And(negate ? filter.Not() : filter);
        return true;
    }

    private static (bool negate, string key, string value) ParseKeyValue(ReadOnlySpan<char> part)
    {
        var delimiterIndex = part.IndexOf(':');
        var key = part[..delimiterIndex];
        var value = part[(delimiterIndex + 1)..];

        if (key[0] != '!')
        {
            return (false, key.ToString(), value.ToString());
        }

        key = key[1..];
        return (true, key.ToString(), value.ToString());
    }

    private static Expression<Func<Assembly, bool>>? ExpressionFromKeyValue(string key, string value)
    {
        return key switch
        {
            "to" => a => a.References.Any(r => CreateStringPredicate(value)(r.To.Name)),
            "by" => a => a.ReferencedBy.Any(r => CreateStringPredicate(value)(r.From.Name)),
            "source" => a =>
                a.Source.ToString().StartsWith(value, StringComparison.OrdinalIgnoreCase),
            "is" => value switch
            {
                "conflict" => a => a.Level >= NoteLevel.Info,
                "unreferenced" => a => a.IsUnreferenced,
                "system" => a => a.IsSystem || a.IsNetApi,
                _ => null
            },
            _ => null
        };
    }

    private static Func<string, bool> CreateStringPredicate(string filter)
    {
        switch (filter[0])
        {
            case '^' when filter[^1] == '$':
            {
                return str => str.Equals(filter[1..^1], StringComparison.OrdinalIgnoreCase);
            }
            case '^':
            {
                return str => str.StartsWith(filter[1..], StringComparison.OrdinalIgnoreCase);
            }
            default:
            {
                if (filter[^1] == '$')
                {
                    return str => str.EndsWith(filter[..^1], StringComparison.OrdinalIgnoreCase);
                }

                return str => str.Contains(filter, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}