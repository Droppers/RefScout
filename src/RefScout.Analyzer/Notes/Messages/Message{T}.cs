using System;
using System.Diagnostics.CodeAnalysis;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages;

internal abstract class Message<TContext> : Message where TContext : IContext
{
    public abstract override NoteType Type { get; }

    public sealed override bool Test(IContext context, Assembly assembly) =>
        context is TContext analyzerContext && Test(analyzerContext, assembly);

    public sealed override bool Test(IContext context, AssemblyRef reference) =>
        context is TContext analyzerContext && Test(analyzerContext, reference);

    public sealed override string Generate(IContext context, Assembly assembly) =>
        context is TContext analyzerContext
            ? Generate(analyzerContext, assembly)
            : throw new NotSupportedException(
                $"This note generator is made contexts derived of {typeof(TContext).Name}, {context.GetType()} is not derived.");

    public sealed override string Generate(IContext context, AssemblyRef reference) =>
        context is TContext analyzerContext
            ? Generate(analyzerContext, reference)
            : throw new NotSupportedException(
                $"This note generator is made contexts derived of {typeof(TContext).Name}, {context.GetType()} is not derived.");

    [ExcludeFromCodeCoverage]
    public virtual bool Test(TContext context, Assembly assembly) => false;

    [ExcludeFromCodeCoverage]
    public virtual string Generate(TContext context, Assembly assembly) =>
        throw new NotSupportedException("Implement the assembly generate method to support note for assemblies.");

    [ExcludeFromCodeCoverage]
    public virtual bool Test(TContext context, AssemblyRef reference) => false;

    [ExcludeFromCodeCoverage]
    public virtual string Generate(TContext context, AssemblyRef reference) =>
        throw new NotSupportedException("Implement the reference generate method to support note for references.");
}