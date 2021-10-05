using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment;

namespace RefScout.Analyzer;

public interface IAnalyzerResult
{
    IReadOnlyList<Assembly> Assemblies { get; }
    EnvironmentInfo EnvironmentInfo { get; }
}