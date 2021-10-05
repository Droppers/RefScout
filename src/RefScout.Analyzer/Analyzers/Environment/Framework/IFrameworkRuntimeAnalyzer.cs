using System.Collections.Generic;

namespace RefScout.Analyzer.Analyzers.Environment.Framework;

internal interface IFrameworkRuntimeAnalyzer : IRuntimeAnalyzer<FrameworkRuntimeAnalyzerResult>
{
    FrameworkRuntime? FindRuntime(IEnumerable<TargetFramework> targets);
}