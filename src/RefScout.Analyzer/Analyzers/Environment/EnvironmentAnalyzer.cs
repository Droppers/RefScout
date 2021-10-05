using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Analyzers.Environment.Framework;
using RefScout.Analyzer.Analyzers.Environment.Mono;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Analyzers.Environment;

// TODO: Refactor EnvironmentAnalyzer entirely at some point
internal class EnvironmentAnalyzer : IEnvironmentAnalyzer
{
    private readonly IEnvironment _environment;

    [ExcludeFromCodeCoverage]
    public EnvironmentAnalyzer(IEnvironment environment, IFileSystem fileSystem) : this(environment,
        new CoreRuntimeAnalyzer(environment, fileSystem),
        new FrameworkRuntimeAnalyzer(),
        new MonoRuntimeAnalyzer(environment, fileSystem)) { }

    public EnvironmentAnalyzer(
        IEnvironment environment,
        ICoreRuntimeAnalyzer coreRuntimeAnalyzer,
        IFrameworkRuntimeAnalyzer frameworkRuntimeAnalyzer,
        IMonoRuntimeAnalyzer monoRuntimeAnalyzer)
    {
        _environment = environment;
        CoreRuntimeAnalyzer = coreRuntimeAnalyzer;
        FrameworkRuntimeAnalyzer = frameworkRuntimeAnalyzer;
        MonoRuntimeAnalyzer = monoRuntimeAnalyzer;
    }

    public ICoreRuntimeAnalyzer CoreRuntimeAnalyzer { get; }

    public IFrameworkRuntimeAnalyzer FrameworkRuntimeAnalyzer { get; }
    public IMonoRuntimeAnalyzer MonoRuntimeAnalyzer { get; }

    public EnvironmentInfo Analyze()
    {
        var framework = _environment.OSVersion.Platform == PlatformID.Win32NT
            ? FrameworkRuntimeAnalyzer.Analyze()
            : null;

        return new EnvironmentInfo
        {
            Core = CoreRuntimeAnalyzer.Analyze(),
            Framework = framework,
            Mono = MonoRuntimeAnalyzer.Analyze()
        };
    }
}