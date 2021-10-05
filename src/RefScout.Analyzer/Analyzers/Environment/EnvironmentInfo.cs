using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Analyzers.Environment.Framework;
using RefScout.Analyzer.Analyzers.Environment.Mono;

namespace RefScout.Analyzer.Analyzers.Environment;

public record EnvironmentInfo(
    CoreRuntimeAnalyzerResult? Core = null,
    FrameworkRuntimeAnalyzerResult? Framework = null,
    MonoRuntimeAnalyzerResult? Mono = null);