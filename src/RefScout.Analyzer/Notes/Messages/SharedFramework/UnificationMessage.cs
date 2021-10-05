using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.SharedFramework;

internal class UnificationMessage : Message<ISharedFrameworkContext>
{
    public override NoteType Type => NoteType.Unification;

    public override bool Test(ISharedFrameworkContext context, Assembly assembly) => assembly.IsUnification;

    public override string Generate(ISharedFrameworkContext context, Assembly assembly) =>
        $"The runtime resolved {assembly.OriginalVersion} to {assembly.Version} using the unification table.";
}