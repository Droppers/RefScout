using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Analyzers.Compatibility;

internal interface ICompatibilityAnalyzerFactory
{
    CompatibilityAnalyzer Create(IContext context, IVersionComparer comparer);
}