using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefScout.Visualizers.Dot.Compiler;

internal class DotGraph : IDotCompilable
{
    private readonly Dictionary<string, string> _attributes = new();
    private readonly List<IDotCompilable> _elements = new();

    public string Compile(int indentation)
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph DependencyGraph {");
        foreach (var attribute in _attributes.Select(a => $"{a.Key}={DotHelpers.Quote(a.Value)}"))
        {
            sb.AppendLine(attribute);
        }

        foreach (var element in _elements)
        {
            sb.AppendLine(element.Compile(indentation));
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    public void AddAttribute(string key, string value)
    {
        _attributes.Add(key, value);
    }

    public void Add(IDotCompilable element)
    {
        _elements.Add(element);
    }

    public void AddRange(IEnumerable<IDotCompilable> elements)
    {
        foreach (var element in elements)
        {
            _elements.Add(element);
        }
    }
}