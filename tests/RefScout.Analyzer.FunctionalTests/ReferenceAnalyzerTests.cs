using System;
using System.IO;
using RefScout.Analyzer.Context;
using Xunit;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
namespace RefScout.Analyzer.FunctionalTests;

public class ReferenceAnalyzerTests
{
    private static readonly string TestDataRoot = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\TestData");
    private static readonly AnalyzeMode[] Modes = { AnalyzeMode.App, AnalyzeMode.AppDirectSystem, AnalyzeMode.All };

    [Fact]
    public void Require_TestData()
    {
        var submoduleCloned = Directory.Exists(TestDataRoot);
        Assert.True(submoduleCloned, "Execute command 'git submodule update --init' to run functional tests.");
    }

    [Theory]
    [InlineData("ILSpy", "ILSpy.exe")]
    [InlineData("dnSpy", "dnSpy.exe")]
    [InlineData("dnSpy", "dnSpy.Console.exe")]
    [InlineData("ShareX", "ShareX.exe")]
    public void Analyze_Framework(string name, string entryPoint)
    {
        RunAnalyzer(Target.Framework, name, entryPoint,
            (result, _) => { Assert.IsType<FrameworkAnalyzerResult>(result); });
    }

    [Theory]
    [InlineData("RoslynPad", "RoslynPad.exe")]
    [InlineData("Fusion++", "Fusion++.exe")]
    [InlineData("paint.net", "paintdotnet.exe")]
    [InlineData("dnSpy", "dnSpy.exe")]
    [InlineData("dnSpy", "dnSpy.Console.exe")]
    [InlineData("SingleFile", "Compression.exe")]
    [InlineData("SingleFile", "NoCompression.exe")]
    public void Analyze_Core(string name, string entryPoint)
    {
        RunAnalyzer(Target.Core, name, entryPoint,
            (result, _) => { Assert.IsType<CoreAnalyzerResult>(result); });
    }

    [Theory]
    [InlineData("Sample", "Sample Silverlight Application.dll")]
    public void Analyze_Silverlight(string name, string entryPoint)
    {
        // Silverlight is technically not at all framework runtime :-)
        RunAnalyzer(Target.Silverlight, name, entryPoint,
            (result, _) => { Assert.IsType<FrameworkAnalyzerResult>(result); });
    }

    private static void RunAnalyzer(
        Target target,
        string name,
        string entryPoint,
        Action<IAnalyzerResult, AnalyzerOptions> analyzerFinished)
    {
        var fileName = Path.Combine(TestDataRoot, "Applications", target.ToString(), name, entryPoint);
        var analyzer = new ReferenceAnalyzer();

        foreach (var mode in Modes)
        {
            var options = new AnalyzerOptions
            {
                AnalyzeMode = mode
            };
            var result = analyzer.Analyze(fileName, options);
            analyzerFinished(result, options);
        }
    }

    private enum Target
    {
        Framework,
        Core,
        Silverlight
    }
}