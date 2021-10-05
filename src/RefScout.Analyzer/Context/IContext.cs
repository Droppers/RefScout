using System;
using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Context;

internal interface IContext : IDisposable
{
    public IResolver Resolver { get; }
    EnvironmentInfo EnvironmentInfo { get; }
    Assembly EntryPoint { get; }
    IConfig Config { get; }

    IReadOnlyList<Assembly> Assemblies { get; set; }

    bool Contains(string assemblyName);
    bool Contains(Assembly assembly);
    void Add(Assembly assembly);

    Assembly? Find(AssemblyIdentity identity);
    Assembly Resolve(AssemblyIdentity identity);
}