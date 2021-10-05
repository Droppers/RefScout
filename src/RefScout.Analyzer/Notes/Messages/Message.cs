using System;
using System.Diagnostics.CodeAnalysis;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages;

[ExcludeFromCodeCoverage]
internal abstract class Message : IMessage<Assembly>, IMessage<AssemblyRef>
{
    public abstract NoteType Type { get; }

    public virtual bool Test(IContext context, Assembly assembly) => false;

    public virtual string Generate(IContext context, Assembly assembly) =>
        throw new NotSupportedException();

    public virtual bool Test(IContext context, AssemblyRef reference) => false;

    public virtual string Generate(IContext context, AssemblyRef reference) =>
        throw new NotSupportedException();
}