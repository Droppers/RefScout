using System;
using System.IO;
using System.Linq;
using RefScout.Analyzer.Notes;
using RefScout.Core.Logging;

namespace RefScout.Analyzer.Readers;

internal record ReaderResult(AssemblyIdentity Identity, IMetadataReader MetadataReader);

internal abstract class AssemblyReader : IAssemblyReader
{
    public Assembly Read(string fileName, AssemblySource source, bool applyMetadata = true)
    {
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));

        var (identity, metadataReader) = InternalRead(fileName);
        var assembly = new Assembly(identity, fileName, source);
        return ApplyMetadataIfRequested(assembly, metadataReader, applyMetadata);
    }

    public Assembly ReadOrDefault(
        string fileName,
        AssemblySource source,
        AssemblyIdentity defaultIdentity,
        bool applyMetadata = true)
    {
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
        _ = defaultIdentity ?? throw new ArgumentNullException(nameof(defaultIdentity));

        var (assembly, metadataReader) = ReadAndFallbackToError(fileName, defaultIdentity, source);
        return ApplyMetadataIfRequested(assembly, metadataReader, applyMetadata);
    }


    private (Assembly assembly, IMetadataReader? metadataReader) ReadAndFallbackToError(
        string fileName,
        AssemblyIdentity identity,
        AssemblySource source)
    {
        try
        {
            var result = InternalRead(fileName);
            return (new Assembly(result.Identity, fileName, source), result.MetadataReader);
        }
        catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
        {
            return (new Assembly(identity, fileName, AssemblySource.NotFound), null);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Assembly file \"{fileName}\" could not be read.");

            var assembly = new Assembly(identity, fileName, AssemblySource.Error);
            assembly.AddNote(NoteType.LoadError,
                $"Assembly file could not be read: {e.Message}");
            return (assembly, null);
        }
    }

    private static Assembly ApplyMetadataIfRequested(
        Assembly assembly,
        IMetadataReader? metadataReader,
        bool applyMetadata)
    {
        try
        {
            return applyMetadata && metadataReader != null
                ? ApplyMetadata(assembly, metadataReader)
                : assembly;
        }
        finally
        {
            metadataReader?.Dispose();
        }
    }

    private static Assembly ApplyMetadata(Assembly assembly, IMetadataReader metadataReader)
    {
        assembly.RawReferences = metadataReader.ReadReferences().ToList();
        assembly.TargetFramework = metadataReader.ReadTargetFramework();

        // We don't need this metadata for .NET Apis
        if (!assembly.IsSystem && !assembly.IsNetApi)
        {
            assembly.Kind = metadataReader.ReadKind();
            assembly.SourceLanguage = metadataReader.ReadSourceLanguage();
        }

        var (archString, arch, is64Bit) = metadataReader.ReadProcessorArchitecture();
        assembly.ProcessorArchitectureString = archString;
        assembly.ProcessorArchitecture = arch;
        assembly.Is64Bit = is64Bit;

        return assembly;
    }

    protected abstract ReaderResult InternalRead(string fileName);
}