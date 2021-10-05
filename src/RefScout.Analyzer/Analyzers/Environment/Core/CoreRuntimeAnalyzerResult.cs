using System.Collections.Generic;

namespace RefScout.Analyzer.Analyzers.Environment.Core;

public record CoreRuntimeAnalyzerResult(IReadOnlyList<CoreRuntime> Runtimes);