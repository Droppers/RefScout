using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config.Core;

namespace RefScout.Analyzer.Resolvers;

internal interface IResolverFactory
{
    IResolver CreateCoreResolver(
        string mainAssemblyFileName,
        Assembly assembly,
        CoreConfig config,
        CoreRuntime? runtime);

    IResolver CreateFrameworkResolver(
        string mainAssemblyFileName,
        Assembly assembly,
        IEnumerable<string> probeFolders);

    IResolver CreateMonoResolver(
        string mainAssemblyFileName,
        Assembly assembly,
        IEnumerable<string> probeFolders);
}