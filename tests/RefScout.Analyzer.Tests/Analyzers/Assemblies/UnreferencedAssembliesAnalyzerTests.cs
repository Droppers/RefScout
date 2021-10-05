using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using RefScout.Analyzer.Analyzers.Assemblies;
using RefScout.Analyzer.Tests.Fakes;
using RefScout.Analyzer.Tests.Resolvers.Strategies;
using Xunit;

namespace RefScout.Analyzer.Tests.Analyzers.Assemblies;

public class UnreferencedAssembliesAnalyzerTests
{
    private readonly IAssemblyAnalyzer _analyzer;

    public UnreferencedAssembliesAnalyzerTests()
    {
        var fileSystem = new FakeFileSystem();
        fileSystem.AddFile(GetPath("Managed.dll"), new MockFileData(File.ReadAllBytes(GetPath("Managed.dll"))));
        _analyzer = new UnreferencedAssembliesAnalyzer(fileSystem);
    }

    [Fact]
    public void Analyze()
    {
        var context = new FakeContext();
        context.Resolver.AddSearchDirectory(@"C:\project");
        context.Resolver.AddSearchDirectory(Path.GetDirectoryName(GetPath("Managed.dll"))!);
        _analyzer.Analyze(context, new AnalyzerOptions(), CancellationToken.None);
    }

    private static string GetPath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "TestFiles", "Assemblies", name);
}