using System.Linq;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.Core;

internal class CoreVersionMismatchWarningMessage : Message<ICoreContext>
{
    public override NoteType Type => NoteType.VersionMismatchWarning;

    public override bool Test(ICoreContext context, Assembly assembly) =>
        assembly.ReferencedBy.Any(r => r.Compatibility == ReferenceCompatibility.MismatchBreaking);

    public override string Generate(ICoreContext context, Assembly assembly)
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

    public override bool Test(ICoreContext context, AssemblyRef reference) =>
        reference.Compatibility == ReferenceCompatibility.MismatchBreaking;

    public override string Generate(ICoreContext context, AssemblyRef reference) =>
        $"Major version mismatch between reference and resolved assembly: {reference.To.Version}";
}