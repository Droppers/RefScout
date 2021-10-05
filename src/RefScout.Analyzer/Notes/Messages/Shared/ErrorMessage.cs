using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.Shared;

// Don't generate a message for assemblies, note is added whenever the loading error occurs
internal class ErrorMessage : Message
{
    public override NoteType Type => NoteType.LoadError;

    public override bool Test(IContext context, AssemblyRef reference) =>
        reference.To.Source == AssemblySource.Error;

    public override string Generate(IContext context, AssemblyRef reference) =>
        "Referenced assembly could not be read.";
}