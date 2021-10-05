using System.Threading;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Analyzers.Assemblies;

internal interface IAssemblyAnalyzer
{
    void Analyze(IContext context, AnalyzerOptions options, CancellationToken cancellationToken);
}