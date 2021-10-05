using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.Shared;
using RefScout.Analyzer.Tests.Fakes;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.Shared;

public class ErrorMessageTests
{
    private readonly IContext _context;
    private readonly Message _generator;

    public ErrorMessageTests()
    {
        _context = new FakeContext();
        _generator = new ErrorMessage();
    }

    [Fact]
    public void Type_IsCorrect()
    {
        Assert.Equal(NoteType.LoadError, _generator.Type);
    }

    [Fact]
    public void Reference_Test_True()
    {
        var from = AssHelp.Ass();
        var to = AssHelp.Ass("test", source: AssemblySource.Error);
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