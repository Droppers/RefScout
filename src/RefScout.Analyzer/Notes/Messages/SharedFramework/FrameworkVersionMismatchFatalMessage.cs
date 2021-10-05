using System.Linq;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.SharedFramework;

internal class FrameworkVersionMismatchFatalMessage : Message<ISharedFrameworkContext>
{
    public override NoteType Type => NoteType.VersionMismatchFatal;

    public override bool Test(ISharedFrameworkContext context, Assembly assembly) =>
        assembly.IsStrongNamed &&
        assembly.ReferencedBy.Any(r =>
            r.BindingRedirectStatus == BindingRedirectStatus.Default &&
            r.Compatibility == ReferenceCompatibility.MismatchBreaking);

    public override string Generate(ISharedFrameworkContext context, Assembly assembly)
    {
        var referencedVersions = assembly.ReferencedBy
            .Where(r => r.Compatibility == ReferenceCompatibility.MismatchBreaking)
            .Select(a => a.Version)
            .Distinct().OrderBy(v => v)
            .ToList();

        var versions = string.Join(", ", referencedVersions);
        return referencedVersions.Count == 1
            ? $"Breaking different version of this strong named assembly is referenced: {versions}"
            : $"Breaking different versions of this strong named assembly are referenced: {versions}";
    }

    public override bool Test(ISharedFrameworkContext context, AssemblyRef reference) =>
        reference.Compatibility == ReferenceCompatibility.MismatchBreaking && reference.To.IsStrongNamed &&
        reference.BindingRedirectStatus is not (BindingRedirectStatus.FailedWrongVersion or BindingRedirectStatus
            .Failed);

    public override string Generate(ISharedFrameworkContext context, AssemblyRef reference) =>
        $"Breaking version mismatch between reference and resolved assembly: {reference.To.Version}";
}