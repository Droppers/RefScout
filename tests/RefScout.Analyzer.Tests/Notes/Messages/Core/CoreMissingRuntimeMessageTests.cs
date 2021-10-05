using System;
using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.Core;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.Core;

public class CoreMissingRuntimeMessageTests
{
    private readonly Message _generator;

    public CoreMissingRuntimeMessageTests()
    {
        _generator = new CoreMissingRuntimeMessage();
    }

    [Fact]
    public void Type_IsCorrect()
    {
        Assert.Equal(NoteType.MissingRuntime, _generator.Type);
    }

    [Fact]
    public void Assembly_Test_True_NoRuntimeVersionConfigured()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsEntryPoint = true
        };

        var context = new FakeCoreMessageContext
        {
            Runtime = null
        };
        Assert.True(_generator.Test(context, assembly));
        Assert.NotNull(_generator.Generate(context, assembly));
    }

    [Fact]
    public void Assembly_Test_True_RuntimeNotFound()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsEntryPoint = true,
            TargetFramework = new TargetFramework(NetRuntime.Core, Version.Parse("5.0"))
        };

        var context = new FakeCoreMessageContext
        {
            Runtime = null,
            EnvironmentInfo = new EnvironmentInfo(new CoreRuntimeAnalyzerResult(new List<CoreRuntime>
            {
                new(Version.Parse("3.1.0"), "3.1.0", string.Empty)
            }))
        };
        Assert.True(_generator.Test(context, assembly));
        Assert.NotNull(_generator.Generate(context, assembly));
    }

    [Fact]
    public void Assembly_Test_True_WrongRuntimePack()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsEntryPoint = true,
            TargetFramework = new TargetFramework(NetRuntime.Core, Version.Parse("5.0"))
        };

        var context = new FakeCoreMessageContext
        {
            Runtime = new CoreRuntime(Version.Parse("3.1.0"), "3.1.0", string.Empty),
            Config = new CoreConfig(null!, null!)
            {
                RuntimePack = RuntimePack.WindowsDesktop
            },
            EnvironmentInfo = new EnvironmentInfo(new CoreRuntimeAnalyzerResult(new List<CoreRuntime>
            {
                new(Version.Parse("3.1.0"), "3.1.0", string.Empty)
                {
                    Packs = { RuntimePack.Default }
                }
            }))
        };
        Assert.True(_generator.Test(context, assembly));
        Assert.NotNull(_generator.Generate(context, assembly));
    }

    [Fact]
    public void Assembly_Test_False()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsEntryPoint = false
        };

        var context = new FakeCoreMessageContext
        {
            Runtime = new CoreRuntime(new Version(5, 0, 0), "5.0.0", "")
        };
        Assert.False(_generator.Test(context, assembly));
        Assert.NotNull(_generator.Generate(context, assembly));
    }
}