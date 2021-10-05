using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RefScout.Analyzer;
using RefScout.Core.Logging;
using RefScout.Visualizers;

namespace RefScout.CLI.Commands;

internal abstract class VisualizerCommand<TVisualizer, TOptions> : IVisualizerCommand
    where TVisualizer : Visualizer<TOptions>
    where TOptions : IVisualizerOptions
{
    protected abstract TOptions Options { get; }

    public abstract bool IsConfigured { get; }

    public abstract void Register(CommandLineApplication application);

    public async Task RunVisualizerAsync(IAnalyzerResult result, VisualizeMode mode)
    {
        var visualizer = (TVisualizer?)Activator.CreateInstance(typeof(TVisualizer));
        if (visualizer == null)
        {
            Logger.Error($"Could not instantiate visualizer {typeof(TVisualizer).Name}");
            return;
        }

        await Task.Run(() => visualizer.Visualize(result, mode, Options));
    }
}