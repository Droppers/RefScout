using System;
using RefScout.Analyzer.Readers;

namespace RefScout.Analyzer.Tests.Fakes;

internal class FakeAssemblyReader : IAssemblyReader
{
    public Assembly Read(string fileName, AssemblySource source, bool applyMetadata = true) =>
        throw new NotSupportedException();

    public Assembly ReadOrDefault(
        string fileName,
        AssemblySource source,
        AssemblyIdentity defaultIdentity,
        bool applyMetadata = true) =>
        throw new NotSupportedException();
}