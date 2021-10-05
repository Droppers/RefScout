using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.Shared;

internal class ArchitectureMismatchMessage : Message
{
    public override NoteType Type => NoteType.ArchitectureMismatch;

    public override bool Test(IContext context, Assembly assembly) => assembly.IsArchitectureMismatch;

    public override string Generate(IContext context, Assembly assembly) =>
        $"Platform target ({assembly.ProcessorArchitectureString}) differs from entry point platform target ({context.EntryPoint.ProcessorArchitectureString}).";
}