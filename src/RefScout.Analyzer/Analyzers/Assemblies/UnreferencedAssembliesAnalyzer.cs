using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Analyzers.Assemblies;

internal class UnreferencedAssembliesAnalyzer : IAssemblyAnalyzer
{
    private readonly IFileSystem _fileSystem;

    public UnreferencedAssembliesAnalyzer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void Analyze(IContext context, AnalyzerOptions options, CancellationToken cancellationToken)
    {
        foreach (var directory in context.Resolver.SearchDirectories)
        {
            if (!_fileSystem.Directory.Exists(directory))
            {
                continue;
            }

            FindUnreferencedInDirectory(context, directory, cancellationToken);
        }
    }

    private void FindUnreferencedInDirectory(IContext context, string directory, CancellationToken cancellationToken)
    {
        var directoryInfo = _fileSystem.DirectoryInfo.FromDirectoryName(directory);
        var files = directoryInfo.GetFiles("*.dll", SearchOption.TopDirectoryOnly)
            .Concat(directoryInfo.GetFiles("*.exe", SearchOption.TopDirectoryOnly));

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = Path.GetFileNameWithoutExtension(file.FullName);
            if (context.Contains(name) || ShouldExcludeName(name))
            {
                continue;
            }

            var identity = ReadAssemblyIdentity(file.FullName);
            if (identity == null)
            {
                continue;
            }

            context.Add(new Assembly(identity, file.FullName, AssemblySource.Local)
            {
                IsUnreferenced = true
            });
        }
    }

    private static AssemblyIdentity? ReadAssemblyIdentity(string fileName)
    {
        try
        {
            // Use System.Reflection.AssemblyName.GetAssemblyName rather than Mono.Cecil because it's a lot faster
            var name = AssemblyName.GetAssemblyName(fileName);
            if (name.Name == null || name.Version == null || MicrosoftHelper.IsSystemAssembly(name.Name))
            {
                return null;
            }

            var culture = name.CultureName ?? AssemblyIdentity.CultureNeutral;
            var token = name.GetPublicKeyToken();
            var publicKeyToken = token == null ? PublicKeyToken.Empty : new PublicKeyToken(token);
            return new AssemblyIdentity(name.Name, culture, publicKeyToken, name.Version);
        }
        catch
        {
            return null;
        }
    }

    private static bool ShouldExcludeName(string name)
        => name.StartsWith("api-", StringComparison.Ordinal);
}