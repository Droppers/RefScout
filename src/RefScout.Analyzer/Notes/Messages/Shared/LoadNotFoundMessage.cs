using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.Shared;

internal class LoadNotFoundMessage : Message
{
    public override NoteType Type => NoteType.LoadNotFound;

    public override bool Test(IContext context, Assembly assembly) =>
        assembly.Source == AssemblySource.NotFound;

    public override string Generate(IContext context, Assembly assembly) =>
        "Could not be found in the application's working directory, probing directories or global assembly cache.";

    public override bool Test(IContext context, AssemblyRef reference) => Test(context, reference.To);

    public override string Generate(IContext context, AssemblyRef reference) =>
        "Referenced assembly could not be found.";
}