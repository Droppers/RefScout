namespace RefScout.Analyzer.Readers;

internal interface IAssemblyReader
{
    public Assembly Read(string fileName, AssemblySource source, bool applyMetadata = true);

    public Assembly ReadOrDefault(
        string fileName,
        AssemblySource source,
        AssemblyIdentity defaultIdentity,
        bool applyMetadata = true);
}