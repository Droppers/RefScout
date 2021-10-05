using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.Shared;
using RefScout.Analyzer.Tests.Fakes;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.Shared;

public class ArchitectureMismatchMessageTests
{
    private readonly IContext _context;
    private readonly Message _generator;

    public ArchitectureMismatchMessageTests()
    {
        _context = new FakeContext();
        _generator = new ArchitectureMismatchMessage();
    }

    [Fact]
    public void Type_IsCorrect()
    {
        Assert.Equal(NoteType.ArchitectureMismatch, _generator.Type);
    }

    [Fact]
    public void Assembly_Test_True()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsArchitectureMismatch = true
        };

        Assert.True(_generator.Test(_context, assembly));
        Assert.NotNull(_generator.Generate(_context, assembly));
    }

    [Fact]
    public void Assembly_Test_False()
    {
        var assembly = AssHelp.Ass("test") with
        {
            IsArchitectureMismatch = false
        };

        Assert.False(_generator.Test(_context, assembly));
    }
}