using System;
using System.IO;
using System.Linq;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Analyzers.Environment.Framework;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Context;

internal class ContextFactory : IContextFactory
{
    public IContext Create(
        IConfigParserFactory configParserFactory,
        IEnvironmentAnalyzer environmentAnalyzer,
        IResolverFactory resolverFactory,
        IAssemblyReader reader,
        AnalyzeRuntime useRuntime,
        string assemblyFileName,
        AnalyzerOptions options,
        EnvironmentInfo environmentInfo,
        Assembly entryPoint)
    {
        switch (entryPoint.TargetFramework?.Runtime)
        {
            case NetRuntime.Core or NetRuntime.Standard
                when useRuntime is AnalyzeRuntime.Default or AnalyzeRuntime.Core:
                return CreateCoreContext(configParserFactory, environmentAnalyzer.CoreRuntimeAnalyzer, resolverFactory,
                    reader, assemblyFileName, options,
                    environmentInfo, entryPoint);
            case NetRuntime.Framework or NetRuntime.Silverlight
                when useRuntime is AnalyzeRuntime.Default or AnalyzeRuntime.Framework:
                // Silverlight is not actually framework runtime at all
                return CreateFrameworkContext(configParserFactory, environmentAnalyzer.FrameworkRuntimeAnalyzer,
                    resolverFactory, reader, assemblyFileName, options,
                    environmentInfo, entryPoint);
            case NetRuntime.Framework when useRuntime is AnalyzeRuntime.Default or AnalyzeRuntime.Mono:
                return CreateMonoContext(configParserFactory, resolverFactory, reader, assemblyFileName, options,
                    environmentInfo, entryPoint);
            default:
                throw new NotSupportedException(
                    $"Context for target framework {entryPoint.TargetFramework} cannot be created by {GetType().Name}");
        }
    }

    private static IContext CreateCoreContext(
        IConfigParserFactory configParserFactory,
        ICoreRuntimeAnalyzer coreRuntimeAnalyzer,
        IResolverFactory resolverFactory,
        IAssemblyReader reader,
        string assemblyFileName,
        AnalyzerOptions options,
        EnvironmentInfo environmentInfo,
        Assembly entryPoint)
    {
        var config = configParserFactory.Create<CoreConfig>().ParseFile(assemblyFileName, options.Config);

        var version = config.TargetRuntimeVersion ??
                      entryPoint.TargetFramework?.Version ?? new Version(5, 0);
        var runtime = config.SelfContained
            ? null
            : coreRuntimeAnalyzer.FindRuntime(version,
                config.RuntimeConfig.RuntimeOptions?.RollForward ?? default, entryPoint.Is64Bit);
        var resolver = resolverFactory.CreateCoreResolver(assemblyFileName, entryPoint, config, runtime);
        return new CoreContext(resolver, reader, environmentInfo, config, runtime, entryPoint);
    }

    private static IContext CreateFrameworkContext(
        IConfigParserFactory configParserFactory,
        IFrameworkRuntimeAnalyzer frameworkRuntimeAnalyzer,
        IResolverFactory resolverFactory,
        IAssemblyReader reader,
        string assemblyFileName,
        AnalyzerOptions options,
        EnvironmentInfo environmentInfo,
        Assembly entryPoint)
    {
        var parser = configParserFactory.Create<FrameworkConfig>();
        var config = parser.ParseFile(assemblyFileName, options.Config);

        // Find applicable runtime
        var supportedRuntimes = config.SupportedRuntimes.Count > 0
            ? config.SupportedRuntimes
            : new[] { entryPoint.TargetFramework }!;
        var runtime = frameworkRuntimeAnalyzer.FindRuntime(supportedRuntimes);

        // Parse machine.config
        FrameworkConfig? machineConfig = null;
        if (runtime != null)
        {
            var frameworkDirectoryName = entryPoint.Is64Bit ? "Framework64" : "Framework";
            var machineConfigFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                $@"Microsoft.NET\{frameworkDirectoryName}\v{runtime.RuntimeVersion}\Config\machine.config)");
            machineConfig = parser.ParseFile(assemblyFileName, machineConfigFileName);
        }

        var resolver =
            resolverFactory.CreateFrameworkResolver(assemblyFileName, entryPoint, config.ProbeFolders);
        return new FrameworkContext(resolver, reader, environmentInfo, machineConfig, config,
            supportedRuntimes, runtime,
            entryPoint);
    }

    private static IContext CreateMonoContext(
        IConfigParserFactory configParserFactory,
        IResolverFactory resolverFactory,
        IAssemblyReader reader,
        string assemblyFileName,
        AnalyzerOptions options,
        EnvironmentInfo environmentInfo,
        Assembly entryPoint)
    {
        var parser = configParserFactory.Create<FrameworkConfig>();
        var config = parser.ParseFile(assemblyFileName, options.Config);

        // For now, just pick the first runtime found, not really worth it making it more advanced
        var runtime = environmentInfo.Mono?.Runtimes.FirstOrDefault();
        var resolver =
            resolverFactory.CreateMonoResolver(assemblyFileName, entryPoint, config.ProbeFolders);
        return new MonoContext(resolver, reader, environmentInfo, config, runtime,
            entryPoint);
    }
}