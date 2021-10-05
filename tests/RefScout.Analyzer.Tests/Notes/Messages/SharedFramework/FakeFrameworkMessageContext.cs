using System;
using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Framework;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Tests.Notes.Messages.SharedFramework;

internal class FakeFrameworkMessageContext : IFrameworkContext
{
    public IReadOnlyList<TargetFramework> SupportedRuntimes { get; set; } = new List<TargetFramework>();
    public FrameworkRuntime? Runtime { get; set; }
    public FrameworkConfig Config { get; init; } = new();
    public FrameworkConfig? MachineConfig { get; } = null!;
    public BindingRedirect FindBindingRedirect(string name, Version version) => throw new NotSupportedException();

    public IReadOnlyList<BindingRedirect> FindBindingRedirects(AssemblyIdentity identity) =>
        throw new NotSupportedException();

    public CodeBase FindCodeBase(AssemblyIdentity identity, Version version) => throw new NotSupportedException();

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