using System;
using System.Collections.Generic;

namespace RefScout.Analyzer.Readers;

internal interface IMetadataReader : IDisposable
{
    IEnumerable<AssemblyIdentity> ReadReferences();
    AssemblySourceLanguage ReadSourceLanguage();
    TargetFramework ReadTargetFramework();
    AssemblyKind ReadKind();
    (string architectureString, ProcessorArchitecture architecture, bool is64Bit) ReadProcessorArchitecture();
}