using System.Linq;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.Shared;

internal class VersionMismatchMessage : Message
{
    public override NoteType Type => NoteType.VersionMismatch;

    public override bool Test(IContext context, Assembly assembly) =>
        assembly.ReferencedBy.Any(r =>
            r.BindingRedirectStatus == BindingRedirectStatus.Default &&
            r.Compatibility == ReferenceCompatibility.Mismatch);

    public override string Generate(IContext context, Assembly assembly)
    {
        var referencedVersions = assembly.ReferencedBy
            .Where(r => r.Compatibility == ReferenceCompatibility.Mismatch)
            .Select(a => a.Version)
            .Distinct().OrderBy(v => v)
            .ToList();
        var versions = string.Join(", ", referencedVersions);

        return referencedVersions.Count == 1
            ? $"Different version of this assembly is referenced: {versions}"
            : $"Different versions of this assembly are referenced: {versions}";
    }

    public override bool Test(IContext context, AssemblyRef reference) =>
        reference.Compatibility == ReferenceCompatibility.Mismatch;

    public override string Generate(IContext context, AssemblyRef reference) =>
        $"Version mismatch between reference and resolved assembly: {reference.To.Version}";
}