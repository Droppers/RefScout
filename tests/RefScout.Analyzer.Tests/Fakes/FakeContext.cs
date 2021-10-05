using System;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Config.Framework;

namespace RefScout.Analyzer.Tests.Fakes;

internal class FakeContext : Analyzer.Context.Context
{
    public FakeContext(IConfig? config = null) : base(new FakeAssemblyReader(), new FakeResolver(),
        new EnvironmentInfo(),
        TestEntryPoint)
    {
        Config = config ?? new FrameworkConfig();
    }

    private static Assembly TestEntryPoint => AssHelp.Ass("EntryPoint") with
    {
        ProcessorArchitecture = ProcessorArchitecture.X86,
        ProcessorArchitectureString = "X86",
        Is64Bit = false
    };

    public override IConfig Config { get; }

    public override Assembly Find(AssemblyIdentity identity) => throw new NotSupportedException();

    public override Assembly Resolve(AssemblyIdentity identity) => throw new NotSupportedException();
}