using System;
using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Resolvers;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages;

public class MessageTests
{
    [Fact]
    public void Generate_Assembly()
    {
        var context = new CorrectContext();

        var assembly = AssHelp.Ass();
        var generator = new FakeMessage();
        Assert.True(generator.Test(context, assembly));
        generator.Generate(context, assembly);
    }

    [Fact]
    public void Generate_Reference()
    {
        var context = (IContext)new CorrectContext();

        var reference = AssHelp.Ref(AssHelp.Ass(), AssHelp.Ass());
        var generator = new FakeMessage();
        Assert.True(generator.Test(context, reference));
        generator.Generate(context, reference);
    }

    [Fact]
    public void Generate_Assembly_WrongContext_Throws()
    {
        var context = (IContext)new WrongContext();

        var assembly = AssHelp.Ass();
        var generator = new FakeMessage();
        Assert.False(generator.Test(context, assembly));
        Assert.Throws<NotSupportedException>(() => { generator.Generate(context, assembly); });
    }

    [Fact]
    public void Generate_Reference_WrongContext_Throws()
    {
        var context = new WrongContext();

        var reference = AssHelp.Ref(AssHelp.Ass(), AssHelp.Ass());
        var generator = new FakeMessage();
        Assert.False(generator.Test(context, reference));
        Assert.Throws<NotSupportedException>(() => { generator.Generate(context, reference); });
    }

    private class FakeMessage : Message<ICoreContext>
    {
        public override NoteType Type { get; } = NoteType.ArchitectureMismatch;

        public override bool Test(ICoreContext context, Assembly assembly) => true;
        public override bool Test(ICoreContext context, AssemblyRef reference) => true;

        public override string Generate(ICoreContext context, Assembly assembly) => string.Empty;

        public override string Generate(ICoreContext context, AssemblyRef reference) => string.Empty;
    }

    private class CorrectContext : ICoreContext
    {
        public IResolver Resolver { get; } = null!;
        public IReadOnlyList<Assembly> Assemblies { get; set; } = null!;
        public Assembly EntryPoint { get; } = null!;
        public EnvironmentInfo EnvironmentInfo { get; } = null!;
        public bool Contains(string assemblyName) => throw new NotSupportedException();

        public bool Contains(Assembly assembly) => throw new NotSupportedException();

        public void Add(Assembly assembly)
        {
            throw new NotSupportedException();
        }

        public Assembly Find(AssemblyIdentity identity) => throw new NotSupportedException();

        public Assembly Resolve(AssemblyIdentity identity) => throw new NotSupportedException();

        public CoreRuntime? Runtime { get; } = null;
        public CoreConfig Config { get; } = null!;

        public void Dispose()
        {
            throw new NotSupportedException();
        }
    }

    private class WrongContext : IContext
    {
        public IResolver Resolver { get; } = null!;
        public IConfig Config { get; } = null!;
        public IReadOnlyList<Assembly> Assemblies { get; set; } = null!;
        public Assembly EntryPoint { get; } = null!;
        public EnvironmentInfo EnvironmentInfo { get; } = null!;
        public bool Contains(string assemblyName) => throw new NotSupportedException();

        public bool Contains(Assembly assembly) => throw new NotSupportedException();

        public void Add(Assembly assembly)
        {
            throw new NotSupportedException();
        }

        public Assembly Find(AssemblyIdentity identity) => throw new NotSupportedException();

        public Assembly Resolve(AssemblyIdentity identity) => throw new NotSupportedException();

        public void Dispose()
        {
            throw new NotSupportedException();
        }
    }
}