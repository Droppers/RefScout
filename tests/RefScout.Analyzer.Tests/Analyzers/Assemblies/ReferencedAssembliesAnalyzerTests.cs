using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using RefScout.Analyzer.Analyzers.Assemblies;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Resolvers;
using Xunit;

namespace RefScout.Analyzer.Tests.Analyzers.Assemblies;

public class ReferencedAssembliesAnalyzerTests
{
    [InlineData(AnalyzeMode.App, new[] { "EntryPoint", "ReferenceA", "ReferenceB", "LocalSystem" })]
    [InlineData(AnalyzeMode.AppDirectSystem,
        new[] { "EntryPoint", "ReferenceA", "ReferenceB", "LocalSystem", "mscorlib", "System.Data" })]
    [InlineData(AnalyzeMode.All,
        new[]
        {
            "EntryPoint", "ReferenceA", "ReferenceB", "LocalSystem", "mscorlib", "System.Data", "System.Nested",
            "System.Nested.Deeper"
        })]
    [Theory]
    public void Run_AnalyzeMode_App(AnalyzeMode mode, string[] expectedAssemblies)
    {
        var context = new FakeContext();
        var options = new AnalyzerOptions
            { AnalyzeMode = mode };
        var step = new ReferencedAssembliesAnalyzer();
        step.Analyze(context, options, CancellationToken.None);

        var actual = context.Assemblies.Select(x => x.Name).ToList();
        Assert.Equal(expectedAssemblies.Length, actual.Count);
        Assert.False(expectedAssemblies.ToList().Except(actual).Any(),
            "all expected references should be present in IContext.Assemblies");
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private class FakeContext : IContext
    {
        private List<Assembly> _assemblies;

        public FakeContext()
        {
            EntryPoint = Resolve(new AssemblyIdentity("EntryPoint"));
            _assemblies = new List<Assembly>();

            Add(EntryPoint);
        }

        public IConfig Config { get; } = null!;
        public EnvironmentInfo EnvironmentInfo { get; } = null!;

        public IResolver Resolver => throw new NotSupportedException();

        public IReadOnlyList<Assembly> Assemblies
        {
            get => _assemblies;
            set => _assemblies = value.ToList();
        }

        public Assembly EntryPoint { get; }

        public bool Contains(string assemblyName) =>
            _assemblies.Any(a => a.Name == assemblyName);

        public bool Contains(Assembly assembly)
        {
            return _assemblies.Any(a => a.Name == assembly.Name && a.Version == assembly.Version);
        }

        public void Add(Assembly assembly)
            =>
                _assemblies.Add(assembly);

        public Assembly? Find(AssemblyIdentity identity) =>
            _assemblies.SingleOrDefault(a => a.Name == identity.Name);

        public Assembly Resolve(AssemblyIdentity identity)
        {
            var name = identity.Name;
            return name switch
            {
                "EntryPoint" => AssHelp.Build(name, "1.0.0.0", AssemblySource.Local,
                    new[]
                    {
                        ("ReferenceA", "3.0.0.0"), ("ReferenceB", "2.5.0.0"), ("mscorlib", "4.0.0.0"),
                        ("System.Data", "4.0.0.0"), ("LocalSystem", "4.0.0.0")
                    }),
                "ReferenceA" => AssHelp.Build(name, "3.0.0.0", AssemblySource.Local),
                "ReferenceB" => AssHelp.Build(name, "2.5.0.0", AssemblySource.Local,
                    new[] { ("mscorlib", "4.0.0.0"), ("System.Data", "4.0.0.0") }),
                "LocalSystem" => AssHelp.Build(name, "4.0.0.0", AssemblySource.Local, isSystem: true,
                    localSystem: true),
                "mscorlib" => AssHelp.Build(name, "4.0.0.0", AssemblySource.Gac, isSystem: true),
                "System.Data" => AssHelp.Build(name, "4.0.0.0", AssemblySource.Gac,
                    new[] { ("System.Nested", "4.0.0.0") },
                    true),
                "System.Nested" => AssHelp.Build(name, "4.0.0.0", AssemblySource.Gac,
                    new[] { ("System.Nested.Deeper", "4.0.0.0") }, true),
                "System.Nested.Deeper" => AssHelp.Build(identity.Name, "4.0.0.0", AssemblySource.Gac,
                    isSystem: true),
                _ => throw new ArgumentOutOfRangeException(nameof(identity))
            };
        }

        public IAnalyzerResult ToResult() => throw new NotSupportedException();
        public void Dispose() { }
    }
}