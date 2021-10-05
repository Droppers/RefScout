using System.Linq;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.SharedFramework;

internal class RedirectSuccessMessage : Message<ISharedFrameworkContext>
{
    public override NoteType Type => NoteType.BindingRedirectSuccess;

    public override bool Test(ISharedFrameworkContext context, Assembly assembly) =>
        !assembly.ReferencedBy.Any(r =>
            r.BindingRedirectStatus is BindingRedirectStatus.FailedWrongVersion or BindingRedirectStatus.Failed)
        && assembly.ReferencedBy.Any(r => r.BindingRedirectStatus == BindingRedirectStatus.Success);

    public override string Generate(ISharedFrameworkContext context, Assembly assembly) =>
        "Binding redirect has solved the differences in referenced versions.";

    public override bool Test(ISharedFrameworkContext context, AssemblyRef reference) =>
        reference.BindingRedirectStatus == BindingRedirectStatus.Success;

    public override string Generate(ISharedFrameworkContext context, AssemblyRef reference) =>
        "Binding redirect has solved the incorrect version reference.";
}