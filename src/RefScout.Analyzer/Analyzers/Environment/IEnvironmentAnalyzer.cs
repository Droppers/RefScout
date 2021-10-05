using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Analyzers.Environment.Framework;
using RefScout.Analyzer.Analyzers.Environment.Mono;

namespace RefScout.Analyzer.Analyzers.Environment;

internal interface IEnvironmentAnalyzer
{
    ICoreRuntimeAnalyzer CoreRuntimeAnalyzer { get; }
    IFrameworkRuntimeAnalyzer FrameworkRuntimeAnalyzer { get; }
    IMonoRuntimeAnalyzer MonoRuntimeAnalyzer { get; }

    EnvironmentInfo Analyze();
}