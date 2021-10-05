using System.Linq;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.SharedFramework;

internal class RedirectFailedWrongVersionMessage : Message<ISharedFrameworkContext>
{
    public override NoteType Type => NoteType.BindingRedirectFailed;

    public override bool Test(ISharedFrameworkContext context, Assembly assembly) => assembly.ReferencedBy.Any(r =>
        r.BindingRedirectStatus == BindingRedirectStatus.FailedWrongVersion);

    public override string Generate(ISharedFrameworkContext context, Assembly assembly)
    {
        var bindingRedirect = FindBindingRedirect(assembly);
        return
            $"Binding redirect did not redirect to the correct version: {bindingRedirect?.NewVersion}, expected: {assembly.Version}.";
    }

    public override bool Test(ISharedFrameworkContext context, AssemblyRef reference) =>
        reference.BindingRedirectStatus == BindingRedirectStatus.FailedWrongVersion;

    public override string Generate(ISharedFrameworkContext context, AssemblyRef reference) =>
        "Binding redirect redirects too the incorrect version.";

    private static BindingRedirect? FindBindingRedirect(Assembly assembly)
    {
        return assembly.ReferencedBy.Find(r =>
                r.BindingRedirectStatus == BindingRedirectStatus.FailedWrongVersion)
            ?.BindingRedirect;
    }
}