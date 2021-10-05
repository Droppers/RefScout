using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using RefScout.Analyzer.Helpers;
using RefScout.Core.Logging;
using RefScout.IPC.Client;
#if !DEBUG
using RefScout.Core.Helpers;
#endif

namespace RefScout.Analyzer.Resolvers.Strategies.Framework
{
    internal class FrameworkProxyGacResolverStrategy : IResolverStrategy
    {
        private const string ProcessName = "RefScout.Ipc.FrameworkRuntime.exe";
        private const string ErrorResponse = "error";

        private readonly IEnvironment _environment;
        private readonly IFileSystem _fileSystem;
        private readonly IIpcClient _ipcClient;

        private string? _ipcPath;

        [ExcludeFromCodeCoverage]
        public FrameworkProxyGacResolverStrategy(IEnvironment environment, IFileSystem fileSystem) : this(environment,
            fileSystem, new PipeIpcClient()) { }

        public FrameworkProxyGacResolverStrategy(IEnvironment environment, IFileSystem fileSystem, IIpcClient ipcClient)
        {
            _environment = environment;
            _fileSystem = fileSystem;
            _ipcClient = ipcClient;
        }

        public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
            targetFramework?.Runtime is NetRuntime.Framework &&
            _environment.OSVersion.Platform is PlatformID.Win32NT &&
            identity.IsStrongNamed;

        public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
        {
            var (version, path) = ResolveFromProxy(identity.FullName);
            if (path == null || version == null)
            {
                return null;
            }

            var unification = identity.Version != version;
            if (unification)
            {
                Logger.Info(
                    $"Assembly \"{identity.FullName}\" resolved from GAC using unification from {identity.Version} to {version}.");
            }

            return new AssemblyResolverResult(AssemblySource.Gac, path)
            {
                Unification = unification
            };
        }


        private void StartProxy()
        {
#if DEBUG
            // Use local variant for debug builds
            _ipcPath = Path.Combine(@"..\..\..\..\RefScout.IPC.FrameworkRuntime\bin\Debug\net472",
                ProcessName);
#else
            // Use embedded resource for release builds
            _ipcPath = Path.Combine(_environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RefScout", ProcessName);
            if (!_fileSystem.File.Exists(_ipcPath))
            {
                if (!ResourceHelper.ExtractResourceToFile<FrameworkProxyGacResolverStrategy>(_fileSystem, ProcessName,
                    _ipcPath))
                {
                    throw new Exception("Could not extract framework runtime proxy from assembly resources");
                }
            }
#endif

            _ipcClient.Start(_ipcPath);
        }

        private (Version? version, string? path) ResolveFromProxy(string assemblyName)
        {
            if (!_ipcClient.Started)
            {
                StartProxy();
            }

            try
            {
                var result = _ipcClient.Send(assemblyName);
                if (result == ErrorResponse)
                {
                    return (null, null);
                }

                var split = result.Split('|');
                return (new Version(split[0]), split[1]);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to resolve assembly using the FrameworkRuntime process");
                return (null, null);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_ipcClient.Started)
            {
                _ipcClient.Dispose();
            }
        }
    }
}