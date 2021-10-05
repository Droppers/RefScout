using McMaster.Extensions.CommandLineUtils;
using RefScout.Visualizers.Dot;

namespace RefScout.CLI.Commands;

internal class DotConflictVisualizerCommand : VisualizerCommand<DotConflictVisualizer, DotConflictVisualizerOptions>
{
    private CommandOption? _outputFile;
    private CommandOption? _showFramework;

    protected override DotConflictVisualizerOptions Options => new(_outputFile?.Value() ?? string.Empty)
    {
        ShowTargetFramework = _showFramework?.HasValue() ?? false
    };

    public override bool IsConfigured => _outputFile?.HasValue() ?? false;

    public override void Register(CommandLineApplication application)
    {
        _outputFile =
            application.Option("--dot <filename>", "Export to a GraphViz file.", CommandOptionType.SingleValue);
        _showFramework = application.Option("-dotf|--dot-framework",
            "Include .NET versions in the GraphViz diagram.",
            CommandOptionType.NoValue);
    }
}