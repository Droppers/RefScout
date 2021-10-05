using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Helpers;
using RefScout.Core.Logging;

namespace RefScout.Analyzer.Config.Core;

internal class CoreConfigParser : IConfigParser<CoreConfig>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(),
            new VersionJsonConverter()
        }
    };

    private readonly IFileSystem _fileSystem;

    public CoreConfigParser(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public CoreConfig ParseFile(string assemblyFileName, string? configFileName)
    {
        var basePath = Path.GetDirectoryName(assemblyFileName);
        var runtimeConfigFileName = Path.Combine(basePath!,
            Path.GetFileNameWithoutExtension(assemblyFileName) + ".runtimeconfig.json");
        var depsFileName =
            Path.Combine(basePath!, Path.GetFileNameWithoutExtension(assemblyFileName) + ".deps.json");
        var runtimeConfig = ReadJson<RuntimeConfig>(runtimeConfigFileName) ?? new RuntimeConfig();
        var depsFile = ReadJson<DepsFile>(depsFileName) ?? new DepsFile();

        var packName = runtimeConfig.RuntimeOptions?.Framework?.Name;
        var pack = RuntimePack.Default;
        if (packName != null)
        {
            _ = CoreRuntimeAnalyzer.StringToPack.TryGetValue(packName, out pack);
        }

        return new CoreConfig(runtimeConfig, depsFile)
        {
            SelfContained = runtimeConfig.RuntimeOptions?.IncludedFrameworks.Count > 0,
            RollForward = runtimeConfig.RuntimeOptions?.RollForward ?? RollForwardBehavior.Minor,
            RuntimePack = pack,
            TargetRuntimeVersion = runtimeConfig.RuntimeOptions?.Framework?.Version
        };
    }

    private T? ReadJson<T>(string fileName) where T : class
    {
        if (!_fileSystem.File.Exists(fileName))
        {
            Logger.Info($"JSON configuration file does not exist: {fileName}");
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(_fileSystem.File.ReadAllText(fileName), JsonSerializerOptions);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error while trying to parse JSON configuration: {fileName}");
            return null;
        }
    }
}