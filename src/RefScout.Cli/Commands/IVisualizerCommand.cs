using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using RefScout.Analyzer;
using RefScout.Visualizers;

namespace RefScout.CLI.Commands;

internal interface IVisualizerCommand
{
    bool IsConfigured { get; }

    void Register(CommandLineApplication application);

    Task RunVisualizerAsync(IAnalyzerResult result, VisualizeMode visualizeMode);
}