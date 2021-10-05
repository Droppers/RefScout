using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.Shared;

internal class UnreferencedMessage : Message
{
    public override NoteType Type => NoteType.Unreferenced;

    public override bool Test(IContext context, Assembly assembly) => assembly.IsUnreferenced;

    public override string Generate(IContext context, Assembly assembly) =>
        "The assembly was found in the probing directories but is not referenced by the analyzed application.";
}