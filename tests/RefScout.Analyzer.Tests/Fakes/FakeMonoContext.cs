using System;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Mono;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Tests.Fakes;

internal class FakeMonoContext : SharedFrameworkContext, IMonoContext
{
    public FakeMonoContext(FrameworkConfig? config = null, EnvironmentInfo? environmentInfo = null) : base(
        new FakeResolver(), new FakeAssemblyReader(),
        environmentInfo ?? new EnvironmentInfo(), new FrameworkConfig(), config ?? new FrameworkConfig(),
        TestEntryPoint) { }

    private static Assembly TestEntryPoint => AssHelp.Ass("EntryPoint") with
    {
        ProcessorArchitecture = ProcessorArchitecture.X86,
        ProcessorArchitectureString = "X86",
        Is64Bit = false
    };

    public override Assembly Find(AssemblyIdentity identity) => throw new NotSupportedException();

    public override Assembly Resolve(AssemblyIdentity identity) => throw new NotSupportedException();

    public MonoRuntime? Runtime { get; set; } = null;
}