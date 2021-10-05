using System;
using System.Collections.Generic;
using System.IO;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Readers;
using Xunit;

namespace RefScout.Analyzer.Tests.Readers;

public class AssemblyReaderTests
{
    private readonly FakeReader _reader;

    public AssemblyReaderTests()
    {
        _reader = new FakeReader();
    }

    [Fact]
    public void Read_WithoutMetadata()
    {
        var assembly = _reader.Read("good.dll", AssemblySource.Gac, false);
        Assert.Equal("good", assembly.Name);
        Assert.Null(assembly.ProcessorArchitectureString);
    }

    [Fact]
    public void Read_WitMetadata()
    {
        var assembly = _reader.Read("good.dll", AssemblySource.Gac);
        Assert.Equal("good", assembly.Name);
        Assert.Equal("Any CPU", assembly.ProcessorArchitectureString);
        Assert.Equal(ProcessorArchitecture.Cil, assembly.ProcessorArchitecture);
        Assert.True(assembly.Is64Bit);
        Assert.Equal(AssemblySourceLanguage.CSharp, assembly.SourceLanguage);
        Assert.Equal(AssemblyKind.Windows, assembly.Kind);
        Assert.NotNull(assembly.TargetFramework);
        Assert.Equal(NetRuntime.Core, assembly.TargetFramework!.Runtime);
        Assert.Equal(Version.Parse("5.0"), assembly.TargetFramework!.Version);
        Assert.Contains(assembly.RawReferences, r => r.Name == "FakeIdentity");
    }

    [Fact]
    public void Read_NotFound()
    {
        Assert.Throws<FileNotFoundException>(() => { _reader.Read("notfound.dll", AssemblySource.Gac, false); });
    }

    [Fact]
    public void SafeRead_NotFound()
    {
        var identity = new AssemblyIdentity("TestIdentity");
        var result = _reader.ReadOrDefault("notfound.dll", AssemblySource.Gac, identity, false);
        Assert.Equal(AssemblySource.NotFound, result.Source);
    }

    [Fact]
    public void SafeRead_Error()
    {
        var identity = new AssemblyIdentity("TestIdentity");
        var result = _reader.ReadOrDefault("error.dll", AssemblySource.Gac, identity, false);
        Assert.Equal(AssemblySource.Error, result.Source);
        Assert.True(result.HasNotes);
        Assert.Contains(result.Notes, n => n.Type == NoteType.LoadError);
    }

    private class FakeMetadataReader : IMetadataReader
    {
        public IEnumerable<AssemblyIdentity> ReadReferences()
        {
            yield return new AssemblyIdentity("FakeIdentity");
            yield return new AssemblyIdentity("ThisIsNothing");
            yield return new AssemblyIdentity("ImASheep");
        }

        public AssemblySourceLanguage ReadSourceLanguage() => AssemblySourceLanguage.CSharp;

        public TargetFramework ReadTargetFramework() => new(NetRuntime.Core, Version.Parse("5.0"));

        public AssemblyKind ReadKind() => AssemblyKind.Windows;

        public (string architectureString, ProcessorArchitecture architecture, bool is64Bit)
            ReadProcessorArchitecture() =>
            ("Any CPU", ProcessorArchitecture.Cil, true);

        public void Dispose() { }
    }

    private class FakeReader : AssemblyReader
    {
        protected override ReaderResult InternalRead(string fileName)
        {
            if (fileName == "good.dll")
            {
                var identity = new AssemblyIdentity("good");
                return new ReaderResult(identity, new FakeMetadataReader());
            }

            if (fileName == "notfound.dll")
            {
                throw new FileNotFoundException("File was not found!");
            }

            if (fileName == "error.dll")
            {
                throw new AccessViolationException("a fake exception to simulate loading error");
            }

            throw new NotSupportedException();
        }
    }
}