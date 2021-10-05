using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using RefScout.Analyzer.Readers.Cecil;
using Xunit;

namespace RefScout.Analyzer.Tests.Readers.Cecil;

public class CecilMetadataReaderTests
{
    public static IEnumerable<object[]> AssemblyTestData => new[]
    {
        new object[]
        {
            "System.Data.dll", new[]
            {
                "mscorlib", "System", "System.Xml", "System.Transactions", "System.Core", "System.Configuration",
                "System.Numerics", "System.Runtime.Caching", "System.EnterpriseServices"
            },
            AssemblySourceLanguage.CppCli,
            new TargetFramework(NetRuntime.Framework, Version.Parse("4.0")),
            AssemblyKind.Dll,
            "x64",
            ProcessorArchitecture.Amd64,
            true
        },
        new object[]
        {
            "System.Runtime_Linux.dll", new[]
            {
                "System.Private.CoreLib", "System.Private.Uri"
            },
            AssemblySourceLanguage.Unknown,
            new TargetFramework(NetRuntime.Core, Version.Parse("5.0")),
            AssemblyKind.Dll,
            "x64",
            ProcessorArchitecture.Amd64,
            true
        },
        new object[]
        {
            "ShareX.exe", new[]
            {
                "mscorlib", "System.Drawing", "ShareX.HelpersLib", "ShareX.ScreenCaptureLib",
                "System.Windows.Forms", "System", "ShareX.UploadersLib", "Newtonsoft.Json", "ShareX.HistoryLib",
                "System.IO.Compression", "zxing", "ShareX.ImageEffectsLib", "ShareX.IndexerLib", "ShareX.MediaLib",
                "System.Design", "System.Core", "Microsoft.VisualBasic", "System.Web"
            },
            AssemblySourceLanguage.CSharp,
            new TargetFramework(NetRuntime.Framework, Version.Parse("4.7.2")),
            AssemblyKind.Windows,
            "Any CPU",
            ProcessorArchitecture.Cil,
            true
        },
        new object[]
        {
            "SharpVectors.Runtime.Wpf.dll", new[]
            {
                "System.Runtime", "PresentationFramework", "System.Runtime.InteropServices", "System.Xaml",
                "PresentationCore", "System.ObjectModel", "System.Runtime.Extensions",
                "System.ComponentModel.TypeConverter", "WindowsBase", "System.ComponentModel.Primitives",
                "System.Collections", "System.Xml.ReaderWriter", "System.IO.Compression", "System.ComponentModel",
                "System.Diagnostics.Tools", "System.Diagnostics.Debug", "System.Resources.ResourceManager",
                "System.Configuration.ConfigurationManager", "System.Threading", "System.IO.FileSystem"
            },
            AssemblySourceLanguage.CSharp,
            new TargetFramework(NetRuntime.Core, Version.Parse("3.1")),
            AssemblyKind.Dll,
            "Any CPU",
            ProcessorArchitecture.Cil,
            true
        }
    };

    [Theory]
    [MemberData(nameof(AssemblyTestData))]
    public void ReadAllMetadata(
        string file,
        string[] expectedReferences,
        AssemblySourceLanguage expectedLanguage,
        TargetFramework expectedFramework,
        AssemblyKind expectedKind,
        string expectedArchitectureString,
        ProcessorArchitecture expectedArchitecture,
        bool expect64Bit)
    {
        var path = GetPath(file);
        var definition = AssemblyDefinition.ReadAssembly(path);
        var reader = new CecilMetadataReader(definition);

        // References
        var references = reader.ReadReferences();
        var actualReferences = references.Select(a => a.Name).ToArray();
        Assert.Equal(expectedReferences, actualReferences);

        // Source language
        var actualLanguage = reader.ReadSourceLanguage();
        Assert.Equal(expectedLanguage, actualLanguage);

        // Framework
        var actualFramework = reader.ReadTargetFramework();
        Assert.Equal(expectedFramework, actualFramework);

        // Kind
        var actualKind = reader.ReadKind();
        Assert.Equal(expectedKind, actualKind);

        // Processor architecture
        var (actualArchitectureString, actualArchitecture, actualIs64Bit) =
            reader.ReadProcessorArchitecture();
        Assert.Equal(expectedArchitectureString, actualArchitectureString);
        Assert.Equal(expectedArchitecture, actualArchitecture);
        Assert.Equal(expect64Bit, actualIs64Bit);
    }

    private static string GetPath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "TestFiles", "Assemblies", name);
}