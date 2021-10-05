using System;
using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Tests.Notes.Messages.Core;

internal class FakeCoreMessageContext : ICoreContext
{
    public CoreConfig Config { get; init; } = new(new RuntimeConfig(), new DepsFile());
    public CoreRuntime? Runtime { get; init; }

    public EnvironmentInfo EnvironmentInfo { get; set; } = new();

    public IResolver Resolver => throw new NotSupportedException();
    public IReadOnlyList<Assembly> Assemblies { get; set; } = Array.Empty<Assembly>();

    public Assembly EntryPoint { get; } = AssHelp.Ass("EntryPoint") with
    {
        ProcessorArchitecture = ProcessorArchitecture.X86,
        ProcessorArchitectureString = "str",
        Is64Bit = false
    };

    public bool Contains(string assemblyName) => throw new NotSupportedException();

    public bool Contains(Assembly assembly) => throw new NotSupportedException();

    public void Add(Assembly assembly)
    {
        throw new NotSupportedException();
    }

    public Assembly Find(AssemblyIdentity identity) => throw new NotSupportedException();

    public Assembly Resolve(AssemblyIdentity identity) => throw new NotSupportedException();
    public IAnalyzerResult ToResult() => throw new NotSupportedException();

    public void Dispose() { }
}