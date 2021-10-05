using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Mono;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Context;

public record MonoAnalyzerResult
(
    IReadOnlyList<Assembly> Assemblies,
    FrameworkConfig? MachineConfig,
    FrameworkConfig Config,
    MonoRuntime? Runtime,
    EnvironmentInfo EnvironmentInfo) : IAnalyzerResult;

internal class MonoContext : SharedFrameworkContext, IMonoContext
{
    public MonoContext(
        IResolver resolver,
        IAssemblyReader reader,
        EnvironmentInfo environmentInfo,
        FrameworkConfig config,
        MonoRuntime? runtime,
        Assembly entryPoint) : base(resolver, reader, environmentInfo, null, config, entryPoint)
    {
        Runtime = runtime;
    }

    public MonoRuntime? Runtime { get; }
}