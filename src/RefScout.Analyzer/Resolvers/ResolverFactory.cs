using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Resolvers;

internal class ResolverFactory : IResolverFactory
{
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironmentAnalyzer _environmentAnalyzer;

    public ResolverFactory(
        IEnvironment environment,
        IFileSystem fileSystem,
        IEnvironmentAnalyzer environmentAnalyzer)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _environmentAnalyzer = environmentAnalyzer ?? throw new ArgumentNullException(nameof(environmentAnalyzer));
    }

    public IResolver CreateCoreResolver(
        string mainAssemblyFileName,
        Assembly assembly,
        CoreConfig config,
        CoreRuntime? runtime)
    {
        if (assembly.TargetFramework == null)
        {
            throw new Exception("Resolvers require TargetFramework for entry point assembly to be specified.");
        }

        var resolver = new CoreResolver(_environment, _fileSystem, assembly.TargetFramework, config,
            runtime);

        var workingDirectory = Path.GetDirectoryName(mainAssemblyFileName);
        if (workingDirectory != null)
        {
            resolver.AddSearchDirectory(workingDirectory);
        }

        return resolver;
    }

    public IResolver CreateFrameworkResolver(
        string mainAssemblyFileName,
        Assembly assembly,
        IEnumerable<string> probeFolders)
    {
        if (assembly.TargetFramework == null)
        {
            throw new Exception("Resolvers require TargetFramework for entry point assembly to be specified.");
        }

        var resolver = new FrameworkResolver(_environment, _fileSystem, assembly.TargetFramework, assembly.Is64Bit);
        return AddDirectories(mainAssemblyFileName, probeFolders, resolver);
    }

    public IResolver CreateMonoResolver(
        string mainAssemblyFileName,
        Assembly assembly,
        IEnumerable<string> probeFolders)
    {
        if (assembly.TargetFramework == null)
        {
            throw new Exception("Resolvers require TargetFramework for entry point assembly to be specified.");
        }

        var resolver = new MonoResolver(_fileSystem, _environmentAnalyzer.MonoRuntimeAnalyzer,
            assembly.TargetFramework);
        return AddDirectories(mainAssemblyFileName, probeFolders, resolver);
    }

    private static IResolver AddDirectories(
        string mainAssemblyFileName,
        IEnumerable<string> directories,
        IResolver resolver)
    {
        var workingDirectory = Path.GetDirectoryName(mainAssemblyFileName);
        if (workingDirectory == null)
        {
            return resolver;
        }

        resolver.AddSearchDirectory(workingDirectory);
        foreach (var directory in directories)
        {
            resolver.AddSearchDirectory(Path.Combine(workingDirectory, directory));
        }

        return resolver;
    }
}