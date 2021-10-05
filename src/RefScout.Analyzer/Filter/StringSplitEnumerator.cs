using System;
using System.Diagnostics.CodeAnalysis;

namespace RefScout.Analyzer.Filter;

// A over optimized way of splitting a string, but it allocates zero bytes ¯\_(ツ)_/¯
internal ref struct StringSplitEnumerator
{
    private ReadOnlySpan<char> _str;

    public StringSplitEnumerator(ReadOnlySpan<char> str)
    {
        _str = str;
        Current = default;
    }

    [ExcludeFromCodeCoverage]
    public StringSplitEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        var span = _str;

        var quoted = false;
        var startIndex = -1;
        for (var position = 0; position < span.Length; position++)
        {
            var token = span[position];

            if (startIndex == -1 && token != ' ')
            {
                startIndex = token == '"' ? position + 1 : position;
            }

            if (token == '"')
            {
                quoted = !quoted;
            }

            if (startIndex >= 0 && (!quoted && token is ' ' or '"' || position == span.Length - 1))
            {
                var endIndex = position == span.Length - 1 && token != '"' ? position + 1 : position;
                _str = span[(position + 1)..];
                Current = span[startIndex..endIndex];
                return true;
            }
        }

        return false;
    }

    public ReadOnlySpan<char> Current { get; private set; }
}