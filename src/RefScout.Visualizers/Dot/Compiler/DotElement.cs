using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace RefScout.Visualizers.Dot.Compiler;

internal abstract class DotElement : IDotCompilable
{
    private readonly Dictionary<string, string> _attributes = new();

    public string Compile(int indentation)
    {
        var attributes = string.Join(", ",
            _attributes.Select(item => $"{item.Key}={DotHelpers.Quote(item.Value)}"));
        if (!string.IsNullOrEmpty(attributes))
        {
            attributes = $"[{attributes}]";
        }

        return "\t" + Prefix() + attributes;
    }

    protected abstract string Prefix();

    public void AddAttribute(string name, string value)
    {
        _attributes.Add(name, value);
    }

    public void AddAttribute(string name, int value)
        => AddAttribute(name, value.ToString());

    public void AddAttribute(string name, double value)
        => AddAttribute(name, value.ToString(CultureInfo.InvariantCulture));

    public void AddAttribute(string name, Version value)
        => AddAttribute(name, value.ToString());

    public void AddAttribute(string name, Color color)
    {
        var hex = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        AddAttribute(name, hex);
    }
}