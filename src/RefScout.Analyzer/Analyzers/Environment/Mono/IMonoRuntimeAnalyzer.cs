using System.Collections.Generic;

namespace RefScout.Analyzer.Analyzers.Environment.Mono;

internal interface IMonoRuntimeAnalyzer : IRuntimeAnalyzer<MonoRuntimeAnalyzerResult>
{
    IEnumerable<string> GetRuntimePrefixDirectories();

    IEnumerable<string> GetGacPrefixDirectories();
}