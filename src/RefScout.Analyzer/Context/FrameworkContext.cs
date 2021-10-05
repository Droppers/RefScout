using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Framework;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Context;

public record FrameworkAnalyzerResult
(
    IReadOnlyList<Assembly> Assemblies,
    FrameworkConfig? MachineConfig,
    FrameworkConfig Config,
    FrameworkRuntime? Runtime,
    EnvironmentInfo EnvironmentInfo) : IAnalyzerResult;

internal class FrameworkContext : SharedFrameworkContext, IFrameworkContext
{
    public FrameworkContext(
        IResolver resolver,
        IAssemblyReader reader,
        EnvironmentInfo environmentInfo,
        FrameworkConfig? machineConfig,
        FrameworkConfig config,
        IReadOnlyList<TargetFramework> supportedRuntimes,
        FrameworkRuntime? runtime,
        Assembly entryPoint) : base(resolver, reader, environmentInfo, machineConfig, config, entryPoint)
    {
        SupportedRuntimes = supportedRuntimes;
        Runtime = runtime;
    }

    public IReadOnlyList<TargetFramework> SupportedRuntimes { get; }
    public FrameworkRuntime? Runtime { get; }
}