using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Context;

internal interface IContextFactory
{
    IContext Create(
        IConfigParserFactory configParserFactory,
        IEnvironmentAnalyzer environmentAnalyzer,
        IResolverFactory resolverFactory,
        IAssemblyReader reader,
        AnalyzeRuntime useRuntime,
        string assemblyFileName,
        AnalyzerOptions options,
        EnvironmentInfo environmentInfo,
        Assembly entryPoint);
}