using Mono.Cecil;

namespace RefScout.Analyzer.Readers.Cecil;

internal class CecilAssemblyReader : AssemblyReader
{
    protected override ReaderResult InternalRead(string fileName)
    {
        var definition = AssemblyDefinition.ReadAssembly(fileName);
        return new ReaderResult(MapNameToIdentity(definition.Name), new CecilMetadataReader(definition));
    }

    public static AssemblyIdentity MapNameToIdentity(AssemblyNameReference name) =>
        new(name.Name, string.IsNullOrEmpty(name.Culture) ? AssemblyIdentity.CultureNeutral : name.Culture,
            new PublicKeyToken(name.PublicKeyToken),
            name.Version)
        {
            IsWindowsRuntime = name.IsWindowsRuntime
        };
}