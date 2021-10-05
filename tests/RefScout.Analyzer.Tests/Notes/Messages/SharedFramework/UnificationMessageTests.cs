using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.SharedFramework;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.SharedFramework;

public class UnificationMessageTests
{
    private readonly IContext _context;
    private readonly Message _generator;

    public UnificationMessageTests()
    {
        _context = new FakeFrameworkMessageContext();
        _generator = new UnificationMessage();
    }

    [Fact]
    public void Type_IsCorrect()
    {
        Assert.Equal(NoteType.Unification, _generator.Type);
    }

    [Fact]
    public void Assembly_Test_True()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsUnification = true
        };

        Assert.True(_generator.Test(_context, assembly));
        Assert.NotNull(_generator.Generate(_context, assembly));
    }

    [Fact]
    public void Assembly_Test_False()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsUnification = false
        };

        Assert.False(_generator.Test(_context, assembly));
    }
}