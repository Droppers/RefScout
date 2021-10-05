using System;
using System.IO;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Readers.Cecil;
using Xunit;

namespace RefScout.Analyzer.Tests.Readers.Cecil;

public class CecilAssemblyReaderTests
{
    private readonly AssemblyReader _reader;

    public CecilAssemblyReaderTests()
    {
        _reader = new CecilAssemblyReader();
    }

    [Theory]
    [InlineData("System.Data.dll", "System.Data")]
    [InlineData("System.Runtime_Linux.dll", "System.Runtime")]
    [InlineData("ShareX.exe", "ShareX")]
    [InlineData("SharpVectors.Runtime.Wpf.dll", "SharpVectors.Runtime.Wpf")]
    public void Read(string name, string assemblyName)
    {
        var path = GetPath(name);
        var actual = _reader.Read(path, AssemblySource.Local);

        Assert.EndsWith(name, actual.Path!);
        Assert.EndsWith(assemblyName, actual.Name);
    }

    private static string GetPath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "TestFiles", "Assemblies", name);
}