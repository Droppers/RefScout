using System.Collections.Generic;

namespace RefScout.Analyzer.Analyzers.Environment.Framework;

public record FrameworkRuntimeAnalyzerResult(IReadOnlyList<FrameworkRuntime> Runtimes);