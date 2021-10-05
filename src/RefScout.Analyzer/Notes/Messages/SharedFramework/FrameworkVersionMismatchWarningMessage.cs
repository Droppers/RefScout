using System.Linq;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.SharedFramework;

internal class FrameworkVersionMismatchWarningMessage : Message<ISharedFrameworkContext>
{
    public override NoteType Type => NoteType.VersionMismatchWarning;

    public override bool Test(ISharedFrameworkContext context, Assembly assembly) =>
        !assembly.IsStrongNamed &&
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
            ? $"Major different version of this assembly is referenced: {versions}, major version differences could be incompatible."
            : $"Major different versions of this assembly are referenced: {versions}, major version differences could be incompatible.";
    }

    public override bool Test(ISharedFrameworkContext context, AssemblyRef reference) =>
        reference.Compatibility == ReferenceCompatibility.MismatchBreaking && !reference.To.IsStrongNamed;

    public override string Generate(ISharedFrameworkContext context, AssemblyRef reference) =>
        $"Major version mismatch between reference and resolved assembly: {reference.To.Version}";
}