using System;
using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Framework;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.Framework;
using RefScout.Analyzer.Tests.Notes.Messages.SharedFramework;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.Framework;

public class FrameworkMissingRuntimeMessageTests
{
    private readonly Message _generator;

    public FrameworkMissingRuntimeMessageTests()
    {
        _generator = new FrameworkMissingRuntimeMessage();
    }

    [Fact]
    public void Type_IsCorrect()
    {
        Assert.Equal(NoteType.MissingRuntime, _generator.Type);
    }

    [Fact]
    public void Assembly_Test_True()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsEntryPoint = true
        };

        var context = new FakeFrameworkMessageContext
        {
            EnvironmentInfo = new EnvironmentInfo
            {
                Framework = new FrameworkRuntimeAnalyzerResult(new[]
                    { new FrameworkRuntime(new Version(4, 0), new Version(4, 8)) })
            },
            SupportedRuntimes = new[] { new TargetFramework(NetRuntime.Framework, new Version(4, 8)) }
        };
        Assert.True(_generator.Test(context, assembly));
        Assert.NotNull(_generator.Generate(context, assembly));
    }

    [Fact]
    public void Assembly_Test_True_EnvironmentNotAnalyzed()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsEntryPoint = true
        };

        var context = new FakeFrameworkMessageContext
        {
            EnvironmentInfo = new EnvironmentInfo(),
            Runtime = new FrameworkRuntime(new Version(4, 0), new Version(4, 8))
        };
        Assert.True(_generator.Test(context, assembly));
        Assert.NotNull(_generator.Generate(context, assembly));
    }

    [Fact]
    public void Assembly_Test_False()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsEntryPoint = true
        };

        var context = new FakeFrameworkMessageContext
        {
            EnvironmentInfo = new EnvironmentInfo
                { Framework = new FrameworkRuntimeAnalyzerResult(new List<FrameworkRuntime>()) },
            Runtime = new FrameworkRuntime(new Version(4, 0), new Version(4, 8))
        };
        Assert.False(_generator.Test(context, assembly));
        Assert.NotNull(_generator.Generate(context, assembly));
    }
}