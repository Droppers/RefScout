using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.Shared;
using RefScout.Analyzer.Tests.Fakes;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.Shared;

public class VersionMismatchMessageTests
{
    private readonly IContext _context;
    private readonly Message _generator;

    public VersionMismatchMessageTests()
    {
        _context = new FakeContext();
        _generator = new VersionMismatchMessage();
    }

    [Fact]
    public void Type_IsCorrect()
    {
        Assert.Equal(NoteType.VersionMismatch, _generator.Type);
    }

    [Fact]
    public void Assembly_Test_FalseWhenAllRedirectStatusNonDefault()
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
    public void Assembly_Test_True()
    {
        var referenceA = AssHelp.Ass("ReferenceA");
        var referenceB = AssHelp.Ass("ReferenceB");
        var assembly = AssHelp.Ass("Conflicting");
        assembly.ReferencedBy = new List<AssemblyRef>
        {
            AssHelp.Ref(referenceA, assembly, redirect: BindingRedirectStatus.Failed),
            AssHelp.Ref(referenceB, assembly, compat: ReferenceCompatibility.Mismatch)
        };

        Assert.True(_generator.Test(_context, assembly));
        Assert.NotNull(_generator.Generate(_context, assembly));
    }

    [Fact]
    public void Reference_Test_True()
    {
        var reference = AssHelp.Ref(AssHelp.Ass(), AssHelp.Ass(),
            compat: ReferenceCompatibility.Mismatch);
        Assert.True(_generator.Test(_context, reference));
        Assert.NotNull(_generator.Generate(_context, reference));
    }

    [Fact]
    public void Reference_Test_False()
    {
        var reference = AssHelp.Ref(AssHelp.Ass(), AssHelp.Ass(),
            compat: ReferenceCompatibility.MismatchBreaking);
        Assert.False(_generator.Test(_context, reference));
    }
}