using System;
using Moq;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes;

public class NoteGeneratorTests
{
    [Fact]
    public void Generate()
    {
        var resolverMock = new Mock<IResolver>();
        var readerMock = new Mock<IAssemblyReader>();
        var generator = new NoteGenerator(new FakeEnvironment());
        var ass = AssHelp.Ass() with
        {
            Source = AssemblySource.NotFound,
            IsUnreferenced = true
        };

        var context = new CoreContext(resolverMock.Object, readerMock.Object, null!, new CoreConfig(null!, null!),
            null!, ass);

        var second = AssHelp.Ass("from");
        second.References.Add(new AssemblyRef(second, ass, Version.Parse("1.2.3.4")));
        ass.ReferencedBy.Add(new AssemblyRef(second, ass, Version.Parse("1.2.3.4")));

        context.Add(second);
        generator.Generate(context);
    }
}