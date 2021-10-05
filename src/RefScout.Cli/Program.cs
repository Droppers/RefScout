using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.HelpText;
using RefScout.Analyzer;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.CLI.Commands;
using RefScout.Core.Logging;
using RefScout.Visualizers;

namespace RefScout.CLI;

public static class Program
{
    private static readonly List<IVisualizerCommand> VisualizerCommands = new()
    {
        new ConsoleConflictVisualizerCommand(),
        new DotConflictVisualizerCommand()
    };

    private static CommandArgument? _file;
    private static CommandOption? _config;
    private static CommandOption? _filter;
    private static CommandOption<AnalyzeMode>? _analyzeMode;
    private static CommandOption<AnalyzeRuntime>? _analyzeRuntime;
    private static CommandOption<VisualizeMode>? _visualizeMode;
    private static CommandOption<VersionCompatibilityMode>? _systemVersionsMode;

    public static int Main(string[] args)
    {
        Logger.AddLogger(new ConsoleLogger());
        Logger.Level = LogLevel.Info;

        var app = new CommandLineApplication
        {
            HelpTextGenerator = new DefaultHelpTextGenerator { SortCommandsByName = false }
        };
        app.HelpOption();

        _file = app.Argument("file", "Required. Assembly file to be analyzed (.exe, .dll)").IsRequired();
        _config = app.Option("-c|--config <config>",
            "The config file containing binding redirects, default <file>.dll|exe.config (.NET Framework only).",
            CommandOptionType.SingleValue);

        _filter = app.Option("-f|--filter <filter>", "Specify a query string to filter the results of the analyzer.",
            CommandOptionType.SingleValue);

        _analyzeMode =
            app.Option<AnalyzeMode>("-a|--analyze <mode>",
                "Specify to which degree assembly references should be analyzed.", CommandOptionType.SingleValue);
        _analyzeRuntime =
            app.Option<AnalyzeRuntime>("-r|--runtime <runtime>",
                "Specify which runtime type should be used for analyzing. Useful for analyzing Mono applications on Windows.",
                CommandOptionType.SingleValue);

        _systemVersionsMode = app.Option<VersionCompatibilityMode>("-sv|--system-versions <mode>",
            "Specify how strict version mismatches of system assemblies should be reported.",
            CommandOptionType.SingleValue);

        _visualizeMode =
            app.Option<VisualizeMode>("-v|--visualize <mode>",
                "Specify which type of assemblies should be visualized by the visualizer.",
                CommandOptionType.SingleValue);

        VisualizerCommands.ForEach(a => a.Register(app));

        app.OnExecuteAsync(async _ =>
        {
            var result = await RunAnalyzerAsync();
            if (result == null)
            {
                await app.Error.WriteLineAsync("Analyzer did not run successfully.");
                return;
            }

            var visualizeMode = _visualizeMode.HasValue() ? _visualizeMode.ParsedValue : VisualizeMode.Default;
            var tasks = VisualizerCommands
                .Where(v => v.IsConfigured)
                .ToList()
                .Select(v => v.RunVisualizerAsync(result, visualizeMode));
            await Task.WhenAll(tasks);
        });

        try
        {
            if (args.Length != 0)
            {
                return app.Execute(args);
            }

            app.ShowHelp();
            return 0;
        }
        catch (CommandParsingException cpe)
        {
            app.Error.WriteLine(cpe.Message);
            app.ShowHelp();
            return -1;
        }
        catch (Exception e)
        {
            app.Error.WriteLine(e.Message);
            return -1;
        }
    }

    private static async Task<IAnalyzerResult?> RunAnalyzerAsync()
    {
        if (_file?.Value == null)
        {
            return null;
        }

        try
        {
            return await Task.Run(() => ReferenceAnalyzer.Run(_file.Value, new AnalyzerOptions
            {
                Config = _config?.Value(),
                Filter = _filter?.Value(),
                SystemVersionMode = _systemVersionsMode?.ParsedValue ?? VersionCompatibilityMode.Off,
                AnalyzeMode = _analyzeMode?.ParsedValue ?? AnalyzeMode.AppDirectSystem,
                AnalyzeRuntime = _analyzeRuntime?.ParsedValue ?? AnalyzeRuntime.Default
            }));
        }
        catch (Exception e)
        {
            Logger.Error(e, "Analyzer failed to run");
            Console.WriteLine(e);
            return null;
        }
    }
}