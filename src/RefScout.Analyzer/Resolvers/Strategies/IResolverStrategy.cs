using System;

namespace RefScout.Analyzer.Resolvers.Strategies;

public interface IResolverStrategy : IDisposable
{
    bool Test(AssemblyIdentity identity, TargetFramework? targetFramework);

    AssemblyResolverResult? Resolve(AssemblyIdentity identity);

    void IDisposable.Dispose() { }
}