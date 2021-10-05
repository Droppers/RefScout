using McMaster.Extensions.CommandLineUtils;
using RefScout.Visualizers.Console;

namespace RefScout.CLI.Commands;

internal class ConsoleConflictVisualizerCommand : VisualizerCommand<ConsoleVisualizer, ConsoleVisualizerOptions>
{
    private CommandOption? _showFramework;

    public override bool IsConfigured => true;

    protected override ConsoleVisualizerOptions Options => new()
    {
        Detailed = _showFramework?.HasValue() ?? false
    };

    public override void Register(CommandLineApplication application)
    {
        _showFramework = application.Option("-cd|--console-detail",
            "Show a more detailed information in the console such as target framework and source language.",
            CommandOptionType.NoValue);
    }
}