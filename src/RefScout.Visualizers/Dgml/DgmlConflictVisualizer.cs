using System.Collections.Generic;
using System.Linq;
using OpenSoftware.DgmlTools;
using OpenSoftware.DgmlTools.Builders;
using OpenSoftware.DgmlTools.Model;
using RefScout.Analyzer;
using RefScout.Analyzer.Notes;

namespace RefScout.Visualizers.Dgml;

public record DgmlConflictVisualizerOptions(string OutputFile) : IVisualizerOptions
{
    public bool ShowTargetFramework { get; init; }
}

public class DgmlConflictVisualizer : Visualizer<DgmlConflictVisualizerOptions>
{
    protected override void RunVisualizer(
        IAnalyzerResult result,
        IReadOnlyList<Assembly> assemblies,
        VisualizeMode mode,
        DgmlConflictVisualizerOptions options)
    {
        var dgmlBuilder = new DgmlBuilder
        {
            NodeBuilders = new NodeBuilder[]
            {
                new NodeBuilder<Assembly>(CreateAssemblyNode),
                new NodeBuilder<Assembly>(a => new Node
                {
                    Id = a.FullName + "_note",
                    Label = GetCommentForAssembly(a),
                    Category = "Note"
                }, a => a.Notes.Count > 0),
                new NodeBuilder<TargetFramework>(f => new Node
                {
                    Id = f.ToString(),
                    Label = f.ToString(),
                    Category = "TargetFramework"
                }, _ => options.ShowTargetFramework)
            },
            LinkBuilders = new LinkBuilder[]
            {
                new LinkBuilder<Assembly>(a => new Link
                {
                    Category = "Note",
                    Source = a.FullName,
                    Target = a.FullName + "_note"
                }, a => a.Notes.Count > 0),
                new LinksBuilder<Assembly>(a => CreateReferenceLinks(a, options.ShowTargetFramework))
            }
        };

        var nodes = new List<object>();
        nodes.AddRange(assemblies.Where(a => !a.IsUnreferenced));
        nodes.AddRange(GetTargetFrameworks(assemblies));

        var graph = dgmlBuilder.Build(nodes);
        graph.Styles = DgmlStyles.ConflictStyles.ToList();

        AddUnreferencedAssemblies(graph, assemblies);
        WriteToFile(options.OutputFile, XmlHelper.SerializeObjectUtf8(graph));
    }

    private static IEnumerable<TargetFramework> GetTargetFrameworks(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .Where(a => !a.IsUnreferenced)
            .Select(a => a.TargetFramework)
            .Where(t => t != null)
            .GroupBy(p => p!.Id)
            .Select(g => g.FirstOrDefault())
            .Cast<TargetFramework>()
            .ToList();
    }

    private static string GetCommentForAssembly(Assembly assembly)
    {
        var result = string.Empty;
        foreach (var comment in assembly.Notes)
        {
            result += comment.Message + "\n";
        }

        return result.Trim();
    }

    private static void AddUnreferencedAssemblies(DirectedGraph graph, IEnumerable<Assembly> assemblies)
    {
        var unreferencedAssemblies =
            assemblies.Where(a => a.IsUnreferenced).ToList();
        if (unreferencedAssemblies.Count == 0)
        {
            return;
        }

        graph.Nodes.Add(new Node
        {
            Id = "UnreferencedAssemblies",
            Category = "UnreferencedAssembliesGroup",
            Label = "Unreferenced Assemblies",
            Group = "Expanded"
        });

        graph.Nodes.Add(new Node
        {
            Id = "UnreferencedAssemblies_comment",
            Label = unreferencedAssemblies.Count == 1
                ? "There was 1 unreferenced assembly found, this assembly was found in the directory of the used assemblies."
                : $"There were {unreferencedAssemblies.Count} unreferenced assemblies found, these assemblies were found in the directory of the used assemblies.",
            Category = "Comment"
        });
        graph.Links.Add(new Link
        {
            Category = "Comment",
            Source = "UnreferencedAssemblies",
            Target = "UnreferencedAssemblies_comment"
        });

        foreach (var assembly in unreferencedAssemblies)
        {
            graph.Nodes.Add(new Node
            {
                Id = assembly.FullName,
                Label = assembly.Name,
                Category = "UnreferencedAssemblies"
            });
            graph.Links.Add(new Link
            {
                Source = "UnreferencedAssemblies",
                Target = assembly.FullName,
                Category = "Contains"
            });
        }
    }

    private static Node CreateAssemblyNode(Assembly assembly) =>
        new()
        {
            Id = assembly.FullName,
            Label = $"{assembly.Name}\n{assembly.Version}",
            Category = assembly.Level <= NoteLevel.Default &&
                       assembly.Source is AssemblySource.Gac or AssemblySource.Shared
                ? "Assembly_GAC"
                : $"Assembly_{assembly.Level}"
        };

    private static IEnumerable<Link> CreateReferenceLinks(Assembly assembly, bool showTargetFramework)
    {
        var referencedBy = assembly.ReferencedBy.ToList();
        var showVersion = referencedBy.Any(a => a.Level == NoteLevel.Warning);
        foreach (var reference in referencedBy)
        {
            var versionString = reference.BindingRedirect != null
                ? $"{reference.BindingRedirect.NewVersion} ({reference.Version})"
                : reference.Version.ToString();

            yield return new Link
            {
                Source = reference.From.FullName,
                Target = assembly.FullName,
                Label = showVersion ? versionString : string.Empty,
                Category = $"Assembly_{reference.Level}"
            };
        }

        if (showTargetFramework && assembly.TargetFramework != null)
        {
            yield return new Link
            {
                Source = assembly.FullName,
                Target = assembly.TargetFramework.ToString(),
                Category = "TargetFramework"
            };
        }
    }
}