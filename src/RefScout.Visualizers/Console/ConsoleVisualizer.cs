using System;
using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Core.Helpers;
using SysConsole = System.Console;

namespace RefScout.Visualizers.Console;

public class ConsoleVisualizerOptions : IVisualizerOptions
{
    public bool Detailed { get; init; } = true;
}

public class ConsoleVisualizer : Visualizer<ConsoleVisualizerOptions>
{
    private const string Tab = "    ";

    private const ConsoleColor ColorText = ConsoleColor.White;
    private const ConsoleColor ColorGac = ConsoleColor.DarkMagenta;
    private const ConsoleColor ColorInfo = ConsoleColor.Cyan;
    private const ConsoleColor ColorWarning = ConsoleColor.Yellow;
    private const ConsoleColor ColorFatal = ConsoleColor.Red;
    private const ConsoleColor ColorSuccess = ConsoleColor.Green;

    protected override bool IsTreeVisualization => false;
    protected override VisualizeMode DefaultMode => VisualizeMode.Conflicts;

    protected override void RunVisualizer(
        IAnalyzerResult result,
        IReadOnlyList<Assembly> assemblies,
        VisualizeMode mode,
        ConsoleVisualizerOptions options)
    {
        WriteEnvironmentInfo(result);

        WriteLegend();

        var conflictsWereFound = false;
        foreach (var assembly in assemblies.Where(a => a.Level != NoteLevel.Message))
        {
            var nonSystemIfApp = mode == VisualizeMode.App && !(assembly.IsSystem || assembly.IsNetApi);
            var displayAssembly = mode == VisualizeMode.All || IsConflictAssembly(assembly) || nonSystemIfApp;
            if (assembly.IsEntryPoint)
            {
                WriteEntryPoint(assembly);
                SysConsole.WriteLine();
            }
            else if (displayAssembly)
            {
                conflictsWereFound = true;
                WriteAssembly(assembly, options.Detailed);
                SysConsole.WriteLine();
            }
        }

        var frameworkConfig = result is FrameworkAnalyzerResult analyzerResult ? analyzerResult.Config : null;
        if (frameworkConfig != null)
        {
            WriteBindingConfigErrors(frameworkConfig);
        }

        if (mode == VisualizeMode.All || conflictsWereFound || frameworkConfig?.ErrorReport.HasErrors == true)
        {
            return;
        }

        ConsoleHelper.WriteLine("No conflicts were found.",
            ColorSuccess);
        SysConsole.WriteLine();
    }

    private static void WriteLegend()
    {
        SysConsole.WriteLine();
        ConsoleHelper.Write("Legend: ");
        ConsoleHelper.Write("Success", ColorSuccess);
        ConsoleHelper.Write(", ");
        ConsoleHelper.Write("Info", ColorInfo);
        ConsoleHelper.Write(", ");
        ConsoleHelper.Write("Warning", ColorWarning);
        ConsoleHelper.Write(", ");
        ConsoleHelper.WriteLine("Fatal", ColorFatal);
        SysConsole.WriteLine();
    }

    private static void WriteEnvironmentInfo(IAnalyzerResult result)
    {
        SysConsole.WriteLine();

        ConsoleHelper.WriteLine("Environment information:", ConsoleColor.White);
        ConsoleHelper.WriteLine("  .NET Core: ", ConsoleColor.White);

        if (result is CoreAnalyzerResult coreAnalyzerResult)
        {
            ConsoleHelper.Write("  Used runtime version: ", ConsoleColor.White);
            if (coreAnalyzerResult.Config.SelfContained)
            {
                ConsoleHelper.WriteLine("self-contained application");
            }
            else if (coreAnalyzerResult.Runtime != null)
            {
                ConsoleHelper.Write(coreAnalyzerResult.Runtime.VersionName);
                ConsoleHelper.WriteLine($" (policy: {coreAnalyzerResult.Config.RollForward})");
            }
            else
            {
                ConsoleHelper.WriteLine("Could not find appropriate runtime version.");
            }
        }

        if (result.EnvironmentInfo.Core != null)
        {
            var x64Runtimes = result.EnvironmentInfo.Core.Runtimes.Where(x => x.Is64Bit).ToList();
            var x86Runtimes = result.EnvironmentInfo.Core.Runtimes.Where(x => !x.Is64Bit).ToList();
            ConsoleHelper.Write("    Installed versions: ", ConsoleColor.White);
            ConsoleHelper.WriteLine(x64Runtimes.Count > 0
                ? string.Join(", ", x64Runtimes.Select(s => s.VersionName))
                : "None installed.");

            ConsoleHelper.Write("    Installed versions (x86): ", ConsoleColor.White);
            ConsoleHelper.WriteLine(x86Runtimes.Count > 0
                ? string.Join(", ", x86Runtimes.Select(s => s.VersionName))
                : "None installed.");
        }

        if (result.EnvironmentInfo.Framework?.Runtimes.Count > 0)
        {
            ConsoleHelper.WriteLine("  .NET Framework: ", ConsoleColor.White);
            foreach (var (framework, version, servicePack) in result.EnvironmentInfo.Framework.Runtimes)
            {
                ConsoleHelper.WriteLine(
                    $"    {framework}: {version} {(servicePack > 0 ? $"(service pack {servicePack})" : "")}");
            }
        }
    }

    private static void WriteEntryPoint(Assembly assembly)
    {
        ConsoleHelper.Write("Entry assembly: ");
        ConsoleHelper.WriteLine(assembly.FullName, ColorText);

        foreach (var (_, to, version) in assembly.References)
        {
            ConsoleHelper.WriteLine($"{Tab}{to.Name} -> {version}", ColorText);
        }
    }

    private static void WriteAssembly(Assembly assembly, bool detailed)
    {
        ConsoleHelper.Write("Reference: ");
        var assemblyColor = GetNoteLevelColor(assembly.Level);
        ConsoleHelper.WriteLine(assembly.FullName, assemblyColor);

        if (detailed)
        {
            if (assembly.TargetFramework != null)
            {
                SysConsole.Write($"Framework: {assembly.TargetFramework}");
            }

            if (assembly.SourceLanguage != AssemblySourceLanguage.Unknown)
            {
                SysConsole.Write($", Language: {assembly.SourceLanguage}");
            }

            SysConsole.WriteLine();
        }

        if (assembly.Path != null)
        {
            SysConsole.WriteLine($"     Path: {assembly.Path}");
        }

        if (assembly.Source is AssemblySource.Gac or AssemblySource.Shared)
        {
            ConsoleHelper.Write("     Note: ");
            if (assembly.Source == AssemblySource.Gac)
            {
                ConsoleHelper.Write("Assembly was found in the ", ColorText);
                ConsoleHelper.WriteLine("GAC",
                    ColorGac);
            }
            else
            {
                ConsoleHelper.Write("Assembly was found in the ", ColorText);
                ConsoleHelper.Write("Shared",
                    ColorGac);
                ConsoleHelper.WriteLine(" runtime folder.", ColorText);
            }
        }

        foreach (var note in assembly.Notes)
        {
            ConsoleHelper.Write("     Note: ");
            ConsoleHelper.WriteLine(note.Message, ColorText);
        }

        foreach (var reference in assembly.ReferencedBy)
        {
            WriteAssemblyReference(reference);
        }
    }

    private static void WriteAssemblyReference(AssemblyRef reference)
    {
        ConsoleHelper.Write(Tab);
        var referenceColor = GetNoteLevelColor(reference.Level);
        ConsoleHelper.Write(reference.Version,
            referenceColor == ColorSuccess ? ColorWarning : referenceColor);

        if (reference.BindingRedirect != null &&
            reference.BindingRedirectStatus != BindingRedirectStatus.Default)
        {
            ConsoleHelper.Write(" -> ");
            ConsoleHelper.Write(reference.ActualVersion, referenceColor);
        }

        ConsoleHelper.Write(" by ");
        ConsoleHelper.WriteLine(reference.From.Name, ColorText);
    }

    private static void WriteBindingConfigErrors(FrameworkConfig config)
    {
        if (config.ErrorReport.HasErrors)
        {
            return;
        }

        SysConsole.WriteLine();
        ConsoleHelper.WriteLine("The applications config file contains invalid configurations:",
            ColorWarning);
        SysConsole.WriteLine(config.ErrorReport.ToString());
    }

    private static bool IsConflictAssembly(Assembly assembly) =>
        assembly.ReferencedBy.Any(a => a.Level != NoteLevel.Default);

    private static ConsoleColor GetNoteLevelColor(NoteLevel level)
        => level switch
        {
            NoteLevel.Info => ColorInfo,
            NoteLevel.Warning => ColorWarning,
            NoteLevel.Fatal => ColorFatal,
            NoteLevel.Success => ColorSuccess,
            _ => ColorText
        };
}