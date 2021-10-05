namespace RefScout.Analyzer.Analyzers.Environment;

internal interface IRuntimeAnalyzer<out TRuntime>
{
    TRuntime Analyze();
}