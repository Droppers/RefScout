using System;
using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment.Mono;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Mono;

public class FakeMonoRuntimeAnalyzer : IMonoRuntimeAnalyzer
{
    public MonoRuntimeAnalyzerResult Analyze() => throw new NotSupportedException();

    public IEnumerable<string> GetRuntimePrefixDirectories()
    {
        yield return @"C:\Program Files\Mono";
    }

    public IEnumerable<string> GetGacPrefixDirectories()
    {
        yield return @"C:\Program Files\Mono";
    }
}