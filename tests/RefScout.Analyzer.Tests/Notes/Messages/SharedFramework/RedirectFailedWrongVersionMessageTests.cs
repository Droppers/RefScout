using System.Collections.Generic;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.SharedFramework;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.SharedFramework;

public class RedirectFailedWrongVersionMessageTests
{
    private readonly IContext _context;
    private readonly Message _generator;

    public RedirectFailedWrongVersionMessageTests()
    {
        _context = new FakeFrameworkMessageContext();
        _generator = new RedirectFailedWrongVersionMessage();
    }

    [Fact]
    public void Type_IsCorrect()
    {
        Assert.Equal(NoteType.BindingRedirectFailed, _generator.Type);
    }

    [Fact]
    public void Assembly_Test_True()
    {
        var referenceA = AssHelp.Ass("ReferenceA");
        var referenceB = AssHelp.Ass("ReferenceB");
        var assembly = AssHelp.Ass("Conflicting");
        assembly.ReferencedBy = new List<AssemblyRef>
        {
            AssHelp.Ref(referenceA, assembly, redirect: BindingRedirectStatus.FailedWrongVersion),
            AssHelp.Ref(referenceB, assembly, redirect: BindingRedirectStatus.Success)
        };

        Assert.True(_generator.Test(_context, assembly));
        Assert.NotNull(_generator.Generate(_context, assembly));
    }

    [Fact]
    public void Assembly_Test_False()
    {
        var referenceA = AssHelp.Ass("ReferenceA");
        var referenceB = AssHelp.Ass("ReferenceB");
        var assembly = AssHelp.Ass("Conflicting");
        assembly.ReferencedBy = new List<AssemblyRef>
        {
            AssHelp.Ref(referenceA, assembly, redirect: BindingRedirectStatus.Failed),
            AssHelp.Ref(referenceB, assembly, redirect: BindingRedirectStatus.Success)
        };

        Assert.False(_generator.Test(_context, assembly));
    }

    [Fact]
    public void Reference_Test_True()
    {
        var reference = AssHelp.Ref(AssHelp.Ass(), AssHelp.Ass(),
            redirect: BindingRedirectStatus.FailedWrongVersion);

        Assert.True(_generator.Test(_context, reference));
        Assert.NotNull(_generator.Generate(_context, reference));
    }

    [Fact]
    public void Reference_Test_False()
    {
        var reference = AssHelp.Ref(AssHelp.Ass(), AssHelp.Ass(),
            redirect: BindingRedirectStatus.Failed);

        Assert.False(_generator.Test(_context, reference));
    }
}