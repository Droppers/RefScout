using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using RefScout.Analyzer.Analyzers.Assemblies;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Filter;
using RefScout.Analyzer.Helpers;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Readers.Cecil;
using RefScout.Analyzer.Resolvers;
using RefScout.Core.Logging;
using SingleFileExtractor.Core;

namespace RefScout.Analyzer;

public class ReferenceAnalyzer : IAnalyzer
{
    private readonly IFileSystem _fileSystem;
    private readonly IAssemblyReader _reader;

    private readonly IResolverFactory _resolverFactory;
    private readonly IConfigParserFactory _configParserFactory;
    private readonly IContextFactory _contextFactory;
    private readonly ICompatibilityAnalyzerFactory _compatibilityAnalyzerFactory;

    private readonly IEnvironmentAnalyzer _environmentAnalyzer;
    private readonly IAssemblyAnalyzer _referencedAssembliesAnalyzer;
    private readonly IAssemblyAnalyzer _unreferencedAssembliesAnalyzer;
    private readonly INoteGenerator _noteGenerator;

    private string? _extractionDirectory;

    public ReferenceAnalyzer()
    {
        IEnvironment environment = new EnvironmentWrapper();
        _fileSystem = new FileSystem();
        _reader = new CecilAssemblyReader();
        _environmentAnalyzer = new EnvironmentAnalyzer(environment, _fileSystem);
        _resolverFactory = new ResolverFactory(environment, _fileSystem, _environmentAnalyzer);
        _configParserFactory = new ConfigParserFactory(_fileSystem);
        _contextFactory = new ContextFactory();
        _compatibilityAnalyzerFactory = new CompatibilityAnalyzerFactory();
        _referencedAssembliesAnalyzer = new ReferencedAssembliesAnalyzer();
        _unreferencedAssembliesAnalyzer = new UnreferencedAssembliesAnalyzer(_fileSystem);
        _noteGenerator = new NoteGenerator(environment);
    }

    internal ReferenceAnalyzer(
        IFileSystem fileSystem,
        IAssemblyReader reader,
        IResolverFactory resolverFactory,
        IConfigParserFactory configParserFactory,
        IContextFactory contextFactory,
        ICompatibilityAnalyzerFactory compatibilityAnalyzerFactory,
        IEnvironmentAnalyzer environmentAnalyzer,
        IAssemblyAnalyzer referencedAssembliesAnalyzer,
        IAssemblyAnalyzer unreferencedAssembliesAnalyzer,
        INoteGenerator noteGenerator)
    {
        _fileSystem = fileSystem;
        _reader = reader;
        _resolverFactory = resolverFactory;
        _configParserFactory = configParserFactory;
        _contextFactory = contextFactory;
        _compatibilityAnalyzerFactory = compatibilityAnalyzerFactory;
        _environmentAnalyzer = environmentAnalyzer;
        _referencedAssembliesAnalyzer = referencedAssembliesAnalyzer;
        _unreferencedAssembliesAnalyzer = unreferencedAssembliesAnalyzer;
        _noteGenerator = noteGenerator;
    }

    public IAnalyzerResult Analyze(
        string fileName,
        AnalyzerOptions? options = null,
        CancellationToken? cancellationToken = null)
    {
        _ = fileName ?? throw new ArgumentNullException(nameof(fileName));
        cancellationToken ??= CancellationToken.None;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name to be analyzed cannot be empty.", nameof(fileName));
        }

        Logger.Info($"Started analyzing \"{fileName}\".");

        options ??= new AnalyzerOptions();
        options.Config ??= $"{fileName}.config";

        // This only applies to .NET Core applications, but at this point we have no idea yet
        // whether it is a framework or core application.
        fileName = ResolveCoreEntryPoint(fileName);

        try
        {
            var entryPoint = ReadEntryPoint(fileName);
            return RunAnalyzer(fileName, options, entryPoint, cancellationToken.Value);
        }
        finally
        {
            CleanUp();
        }
    }

    public static IAnalyzerResult Run(
        string fileName,
        AnalyzerOptions options,
        CancellationToken? cancellationToken = null)
    {
        var analyzer = new ReferenceAnalyzer();
        return analyzer.Analyze(fileName, options, cancellationToken);
    }

    private Assembly ReadEntryPoint(string fileName)
    {
        if (!_fileSystem.File.Exists(fileName))
        {
            throw new Exception($"Assembly file or executable does not exist: \"{fileName}\".");
        }

        return _reader.Read(fileName, AssemblySource.Local) with
        {
            IsEntryPoint = true
        };
    }

    private IAnalyzerResult RunAnalyzer(
        string fileName,
        AnalyzerOptions options,
        Assembly entryPoint,
        CancellationToken cancellationToken)
    {
        var environmentInfo = _environmentAnalyzer.Analyze();

        var comparer = options.VersionComparer ?? new DefaultVersionComparer(options.SystemVersionMode);
        using var context = _contextFactory.Create(_configParserFactory, _environmentAnalyzer, _resolverFactory,
            _reader, options.AnalyzeRuntime, fileName, options, environmentInfo, entryPoint);
        var compatibilityAnalyzer = _compatibilityAnalyzerFactory.Create(context, comparer);

        AnalyzeReferenced(context, options, cancellationToken);
        AnalyzeUnreferenced(context, options, cancellationToken);
        AnalyzeCompatibility(compatibilityAnalyzer);

        GenerateNotes(context);

        Filter(context, options);
        Sort(context);

        return ConvertContextToResult(context);
    }

    private void AnalyzeReferenced(IContext context, AnalyzerOptions options, CancellationToken cancellationToken)
    {
        _referencedAssembliesAnalyzer.Analyze(context, options, cancellationToken);
    }

    private void AnalyzeUnreferenced(IContext context, AnalyzerOptions options, CancellationToken cancellationToken)
    {
        _unreferencedAssembliesAnalyzer.Analyze(context, options, cancellationToken);
    }

    private static void AnalyzeCompatibility(ICompatibilityAnalyzer compatibilityAnalyzer)
    {
        compatibilityAnalyzer.Analyze();
    }

    private void GenerateNotes(IContext context)
    {
        _noteGenerator.Generate(context);
    }

    private static void Filter(IContext context, AnalyzerOptions options)
    {
        if (options.Filter == null)
        {
            return;
        }

        var assemblies = context.Assemblies.ToList();
        var predicate = FilterParser.Parse(options.Filter);
        context.Assemblies = assemblies.Where(predicate).ToList();
    }

    private static void Sort(IContext context)
    {
        var assemblies = context.Assemblies.ToList();
        foreach (var assembly in assemblies)
        {
            assembly.References.Sort((a, b) => string.Compare(a.To.Name, b.To.Name, StringComparison.Ordinal));
            assembly.ReferencedBy.Sort((a, b) =>
                string.Compare(a.From.Name, b.From.Name, StringComparison.Ordinal));
        }

        assemblies = assemblies.OrderBy(assembly =>
        {
            return assembly switch
            {
                { IsEntryPoint: true } => 10,
                { IsUnreferenced: true } => 100,
                not { IsNetApi: true } and not
                    { IsSystem: true } => assembly.ReferencedBy.Any(a => a.From.IsEntryPoint) ? 20 : 25,
                { IsNetApi: true } => 30,
                { IsSystem: true } => 40
            };
        }).ThenBy(a => a.Name).ToList();

        context.Assemblies = assemblies;
    }

    private static IAnalyzerResult ConvertContextToResult(IContext context)
    {
        return context switch
        {
            ICoreContext coreContext => new CoreAnalyzerResult(context.Assemblies, coreContext.Config,
                coreContext.Runtime, coreContext.EnvironmentInfo),
            IMonoContext monoContext => new MonoAnalyzerResult(context.Assemblies,
                monoContext.MachineConfig, monoContext.Config, monoContext.Runtime,
                context.EnvironmentInfo),
            IFrameworkContext frameworkContext => new FrameworkAnalyzerResult(context.Assemblies,
                frameworkContext.MachineConfig, frameworkContext.Config, frameworkContext.Runtime,
                context.EnvironmentInfo),
            _ => throw new NotSupportedException($"Cannot convert {context.GetType().Name} to AnalyzerResult")
        };
    }

    private void CleanUp()
    {
        if (_extractionDirectory == null)
        {
            return;
        }

        try
        {
            Directory.Delete(_extractionDirectory, true);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Could not delete single file extraction directory: {_extractionDirectory}");
        }
    }

    private string ResolveCoreEntryPoint(string fileName)
    {
        var baseDirectory = Path.GetDirectoryName(fileName)!;

        var startupInfo = TryReadStartupInfo(fileName);
        if (startupInfo != null)
        {
            var extractionDirectory = TryExtractCoreSingleFile(fileName);
            if (extractionDirectory != null)
            {
                baseDirectory = extractionDirectory;
                _extractionDirectory = extractionDirectory;
            }
        }

        if (startupInfo != null && startupInfo.EntryPoint != null)
        {
            return Path.Combine(baseDirectory, startupInfo.EntryPoint);
        }

        var originalFileName = GetOriginalFileName(fileName);
        return originalFileName != null ? Path.Combine(baseDirectory, originalFileName) : fileName;
    }

    private static StartupInfo? TryReadStartupInfo(string fileName)
    {
        try
        {
            return new ExecutableReader().ReadStartupInfo(fileName);
        }
        catch (Exception e)
        {
            Logger.Info("Cannot extract .NET Core startup info: " + e.Message);
        }

        return null;
    }

    private string? TryExtractCoreSingleFile(string fileName)
    {
        try
        {
            var extractionDirectory = Path.Join(_fileSystem.Path.GetTempPath(), "RefScout",
                Path.GetFileNameWithoutExtension(fileName));
            _ = BundleExtractor.Extract(fileName, extractionDirectory);
            return extractionDirectory;
        }
        catch (Exception e)
        {
            Logger.Info("Cannot extract single file application: " + e.Message);
        }

        return null;
    }

    private static string? GetOriginalFileName(string fileName)
    {
        try
        {
            var fileInfo = FileVersionInfo.GetVersionInfo(fileName);
            if (fileInfo.OriginalFilename != null && Path.GetFileName(fileName) != fileInfo.OriginalFilename &&
                Path.GetExtension(fileInfo.OriginalFilename) == ".dll")
            {
                return fileInfo.OriginalFilename;
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error while trying to resolve entry assembly filename using FileVersionInfo.");
        }

        return null;
    }
}