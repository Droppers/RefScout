using System;
using System.Collections.Generic;
using System.IO;
using RefScout.Analyzer;
using RefScout.Analyzer.Notes;
using RefScout.Core.Logging;

namespace RefScout.Visualizers;

public abstract class Visualizer<TOptions>
    where TOptions : IVisualizerOptions
{
    protected virtual bool IsTreeVisualization => true;
    protected virtual VisualizeMode DefaultMode => VisualizeMode.All;

    public void Visualize(IAnalyzerResult result, VisualizeMode mode, TOptions options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));
        _ = result ?? throw new ArgumentNullException(nameof(result));

        Logger.Info($"Started running visualizer: {GetType().Name}.");

        var assemblies = result.Assemblies;
        mode = mode == VisualizeMode.Default ? DefaultMode : mode;
        assemblies = mode switch
        {
            VisualizeMode.Conflicts when IsTreeVisualization => AssemblyTreeFilter.OnlyConflictAssemblies(
                result.Assemblies, NoteLevel.Info),
            VisualizeMode.App when IsTreeVisualization => AssemblyTreeFilter.OnlyApplicationAssemblies(
                result.Assemblies),
            _ => assemblies
        };

        RunVisualizer(result, assemblies, mode, options);
    }

    protected abstract void RunVisualizer(
        IAnalyzerResult result,
        IReadOnlyList<Assembly> filteredAssemblies,
        VisualizeMode mode,
        TOptions options);

    protected bool WriteToFile(string? fileName, string content)
    {
        if (fileName == null)
        {
            Logger.Error("Could not write visualizer output to file, no output file was specified.");
            return false;
        }

        Logger.Info($"Writing visualizer result to file \"{fileName}\"");
        try
        {
            var directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fileName, content);
            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Could not write visualizer result to file");
        }

        return false;
    }
}

public enum VisualizeMode
{
    Default,
    All, // Default for others
    Conflicts, // Default for console
    App
}