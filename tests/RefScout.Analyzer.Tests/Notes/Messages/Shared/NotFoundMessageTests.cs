using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.Shared;
using RefScout.Analyzer.Tests.Fakes;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.Shared;

public class NotFoundMessageTests
{
    private readonly IContext _context;
    private readonly Message _generator;

    public NotFoundMessageTests()
    {
        _context = new FakeContext();
        _generator = new LoadNotFoundMessage();
    }

    [Fact]
    public void Type_IsCorrect()
    {
        Assert.Equal(NoteType.LoadNotFound, _generator.Type);
    }

    [Fact]
    public void Assembly_Test_True()
    {
        var assembly = AssHelp.Ass("test", source: AssemblySource.NotFound);
        Assert.True(_generator.Test(_context, assembly));
        Assert.NotNull(_generator.Generate(_context, assembly));
    }

    [Fact]
    public void Assembly_Test_False()
    {
        var assembly = AssHelp.Ass("test", source: AssemblySource.Local);
        Assert.False(_generator.Test(_context, assembly));
    }

    [Fact]
    public void Reference_Test_True()
    {
        var from = AssHelp.Ass();
        var to = AssHelp.Ass("test", source: AssemblySource.NotFound);
        var reference = AssHelp.Ref(from, to);

        Assert.True(_generator.Test(_context, reference));
        Assert.NotNull(_generator.Generate(_context, reference));
    }

    [Fact]
    public void Reference_Test_False()
    {
        var from = AssHelp.Ass();
        var to = AssHelp.Ass("test", source: AssemblySource.Local);
        var reference = AssHelp.Ref(from, to);

        Assert.False(_generator.Test(_context, reference));
    }
}