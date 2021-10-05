using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RefScout.Analyzer;
using RefScout.Analyzer.Notes;
using RefScout.Visualizers.Dot.Compiler;

namespace RefScout.Visualizers.Dot;

public enum GraphDirection
{
    Lr,
    Tb
}

public record DotConflictVisualizerOptions(string OutputFile) : IVisualizerOptions
{
    public bool ShowTargetFramework { get; init; }
    public bool DarkTheme { get; init; }
    public GraphDirection Direction { get; init; } = GraphDirection.Tb;
}

public class DotConflictVisualizer : Visualizer<DotConflictVisualizerOptions>
{
    private IDotTheme _theme = new DotLightTheme();

    protected override void RunVisualizer(
        IAnalyzerResult result,
        IReadOnlyList<Assembly> assemblies,
        VisualizeMode mode,
        DotConflictVisualizerOptions options)
    {
        if (options.DarkTheme)
        {
            _theme = new DotDarkTheme();
        }

        var assemblyList = assemblies.ToList();
        var graph = new DotGraph();
        graph.AddAttribute("rankdir", options.Direction.ToString().ToUpper());
        graph.AddRange(CreateDefaultStyle());
        foreach (var assembly in assemblyList.Where(a => !a.IsUnreferenced))
        {
            graph.AddRange(CompileAssembly(assembly));
        }

        if (options.ShowTargetFramework)
        {
            graph.AddRange(CompileTargetFrameworks(assemblyList));
        }

        WriteToFile(options.OutputFile, graph.Compile(1));
    }

    private IEnumerable<IDotCompilable> CreateDefaultStyle()
    {
        var elements = new List<IDotCompilable>();
        var graph = new DotNode("graph");
        graph.AddAttribute("bgcolor", _theme.Background);
        elements.Add(graph);

        var node = new DotNode("node");
        node.AddAttribute("shape", "box");
        node.AddAttribute("style", "rounded,filled");
        node.AddAttribute("fillcolor", _theme.NodeFill);
        node.AddAttribute("color", _theme.NodeBorder);
        node.AddAttribute("fontname", "Arial");
        node.AddAttribute("fontcolor", _theme.FontColor);
        node.AddAttribute("fontsize", 11);
        node.AddAttribute("height", 0.4);
        elements.Add(node);

        var edge = new DotNode("edge");
        edge.AddAttribute("fontname", "Arial");
        edge.AddAttribute("fontcolor", _theme.FontColor);
        edge.AddAttribute("fontsize", 11);
        edge.AddAttribute("color", _theme.EdgeStroke);
        elements.Add(edge);

        return elements;
    }

    private IEnumerable<IDotCompilable> CompileAssembly(Assembly assembly)
    {
        var elements = new List<IDotCompilable>();
        var node = new DotNode(assembly.Id);
        node.AddAttribute("id", assembly.Id);
        node.AddAttribute("label", $"{assembly.Name}\n{assembly.ActualVersion}");

        var assemblyColor = GetNodeColor(assembly);
        if (assemblyColor != null)
        {
            if (assembly.Source is AssemblySource.Gac or AssemblySource.Shared)
            {
                node.AddAttribute("penwidth", 1.5);
            }
            else
            {
                node.AddAttribute("fillcolor", assemblyColor.Value);
            }

            node.AddAttribute("color", assemblyColor.Value);
        }

        elements.Add(node);

        foreach (var reference in assembly.References)
        {
            var showVersion = reference.To.ReferencedBy.Any(a => a.Level is NoteLevel.Warning);

            var edge = new DotEdge(assembly.Id, reference.To.Id);
            elements.Add(edge);

            var edgeColor = GetEdgeColor(reference);
            if (!showVersion && edgeColor == null)
            {
                continue;
            }

            if (edgeColor != null)
            {
                edge.AddAttribute("fontcolor", edgeColor.Value);
                edge.AddAttribute("color", edgeColor.Value);
            }

            if (showVersion)
            {
                edge.AddAttribute("label", reference.ActualVersion);
            }
        }

        return elements;
    }

    private IReadOnlyList<IDotCompilable> CompileTargetFrameworks(IReadOnlyList<Assembly> assemblies)
    {
        var frameworks = assemblies
            .Where(a => a.TargetFramework != null && !a.IsUnreferenced)
            .Select(a => a.TargetFramework)
            .GroupBy(p => p?.ToString())
            .Select(g => g.First())
            .Cast<TargetFramework>();

        var elements = new List<DotElement>();
        foreach (var framework in frameworks)
        {
            var node = new DotNode(framework.Id);
            node.AddAttribute("label", framework.ToString());
            node.AddAttribute("fillcolor", _theme.NodeFramework);
            node.AddAttribute("color", _theme.NodeFramework);
            node.AddAttribute("fontcolor", _theme.NodeFrameworkText);
            elements.Add(node);
        }

        foreach (var assembly in assemblies.Where(a => !a.IsUnreferenced))
        {
            if (assembly.TargetFramework == null)
            {
                continue;
            }

            var edge = new DotEdge(assembly.Id, assembly.TargetFramework.Id);
            edge.AddAttribute("color", _theme.EdgeFramework);
            edge.AddAttribute("style", "dashed");
            elements.Add(edge);
        }

        return elements;
    }

    private Color? GetNodeColor(Assembly assembly)
    {
        Color? color = assembly.Level switch
        {
            NoteLevel.Info => _theme.NodeInfo,
            NoteLevel.Warning => _theme.NodeWarning,
            NoteLevel.Fatal => _theme.NodeFatal,
            NoteLevel.Success => _theme.NodeSuccess,
            _ => null
        };

        return color ?? (assembly.Source is AssemblySource.Gac or AssemblySource.Shared
            ? _theme.NodeGac
            : null);
    }

    private Color? GetEdgeColor(AssemblyRef reference)
    {
        return reference.Level switch
        {
            NoteLevel.Info => _theme.EdgeInfo,
            NoteLevel.Warning => _theme.EdgeWarning,
            NoteLevel.Fatal => _theme.EdgeFatal,
            NoteLevel.Success => _theme.EdgeSuccess,
            _ => null
        };
    }
}