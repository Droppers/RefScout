using System;
using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Resolvers;
using Xunit;

namespace RefScout.Analyzer.Tests.Analyzers.Compatibility;

public class CompatibilityAnalyzerTests
{
    [Fact]
    public void Run_ArchitectureMismatch_EntryPoint_x64()
    {
        var x86 = AssHelp.Ass("ReferenceA") with
        {
            ProcessorArchitecture = ProcessorArchitecture.X86
        };
        var msil = AssHelp.Ass("ReferenceB") with
        {
            ProcessorArchitecture = ProcessorArchitecture.Cil
        };
        var entryPoint = AssHelp.Ass("EntryPoint") with
        {
            ProcessorArchitecture = ProcessorArchitecture.Amd64
        };

        var context = new FakeContext
        {
            Assemblies = new[] { entryPoint, x86, msil },
            EntryPoint = entryPoint
        };

        var step = new FakeAnalyzer(context, new DefaultVersionComparer(VersionCompatibilityMode.Loose));
        step.Analyze();

        Assert.False(msil.IsArchitectureMismatch);
        Assert.True(x86.IsArchitectureMismatch);
    }

    private class FakeAnalyzer : CompatibilityAnalyzer
    {
        public FakeAnalyzer(IContext context, IVersionComparer comparer) : base(context, comparer) { }
    }

    private class FakeContext : IContext
    {
        public EnvironmentInfo EnvironmentInfo { get; } = null!;
        public IConfig Config { get; } = null!;

        public IResolver Resolver => throw new NotSupportedException();
        public IReadOnlyList<Assembly> Assemblies { get; set; } = new List<Assembly>();
        public Assembly EntryPoint { get; init; } = AssHelp.Ass();

        public bool Contains(string assemblyName) => throw new NotSupportedException();

        public bool Contains(Assembly assembly) => throw new NotSupportedException();

        public void Add(Assembly assembly) => throw new NotSupportedException();

        public Assembly Find(AssemblyIdentity identity) => throw new NotSupportedException();

        public Assembly Resolve(AssemblyIdentity identity) => throw new NotSupportedException();

        public void Dispose() { }
    }
}