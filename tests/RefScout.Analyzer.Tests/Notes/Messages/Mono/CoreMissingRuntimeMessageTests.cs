using System;
using RefScout.Analyzer.Analyzers.Environment.Mono;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.Mono;
using RefScout.Analyzer.Tests.Fakes;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.Mono;

public class MonoMissingRuntimeMessageTests
{
    private readonly Message _generator;

    public MonoMissingRuntimeMessageTests()
    {
        _generator = new MonoMissingRuntimeMessage(new FakeEnvironment());
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

        var context = new FakeMonoContext();
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

        var context = new FakeMonoContext
        {
            Runtime = new MonoRuntime(new Version(6, 12, 0, 0), "", new[] { new Version(4, 5) })
        };
        Assert.False(_generator.Test(context, assembly));
        Assert.NotNull(_generator.Generate(context, assembly));
    }
}