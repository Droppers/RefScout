using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.SharedFramework;

internal class RedirectFailedMessage : Message<ISharedFrameworkContext>
{
    public override NoteType Type => NoteType.BindingRedirectFailed;

    public override bool Test(ISharedFrameworkContext context, Assembly assembly) =>
        assembly.ReferencedBy.Any(r => r.BindingRedirectStatus == BindingRedirectStatus.Failed);

    public override string Generate(ISharedFrameworkContext context, Assembly assembly) =>
        $"Binding redirect did not cover all versions used. Modify the binding redirect old version: \"{DeterminePossibleRedirectSolution(assembly.ReferencedBy)}\".";

    public override bool Test(ISharedFrameworkContext context, AssemblyRef reference) =>
        reference.BindingRedirectStatus == BindingRedirectStatus.Failed;

    public override string Generate(ISharedFrameworkContext context, AssemblyRef reference) =>
        "Binding redirect did not cover this reference.";

    private static string DeterminePossibleRedirectSolution(IEnumerable<AssemblyRef> references)
    {
        var versions = references.Select(a => a.Version)
            .ToList();
        var maxVersion = versions.Max();
        return $"0.0.0.0-{maxVersion}";
    }
}