using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Resolvers.Strategies.Framework;

internal class FusionGacResolverStrategy : IResolverStrategy
{
    private static readonly string[] FrameworkVersions = { "v4.0.30319", "v2.0.50727", "v1.1.4322", "v1.0.3705" };

    private static readonly string[] ArchitectureDependentAssemblies =
    {
        "mscorlib",
        "PresentationCore",
        "System.Data",
        "System.Data.OracleClient",
        "System.EnterpriseServices",
        "System.Printing",
        "System.Transactions",
        "System.Web"
    };

    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;
    private readonly IFusionWrapper _fusionWrapper;
    private readonly bool _is64Bit;

    [ExcludeFromCodeCoverage]
    public FusionGacResolverStrategy(IEnvironment environment, IFileSystem fileSystem, bool is64Bit) : this(environment,
        fileSystem, new FusionWrapper(), is64Bit) { }

    public FusionGacResolverStrategy(
        IEnvironment environment,
        IFileSystem fileSystem,
        IFusionWrapper fusionWrapper,
        bool is64Bit)
    {
        _environment = environment;
        _fileSystem = fileSystem;
        _fusionWrapper = fusionWrapper;
        _is64Bit = is64Bit;

        _fusionWrapper.LoadFusion(FindFusionPaths());
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        targetFramework?.Runtime is NetRuntime.Framework &&
        _environment.OSVersion.Platform is PlatformID.Win32NT &&
        identity.IsStrongNamed;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        var architecture = "MSIL";
        if (ArchitectureDependentAssemblies.Contains(identity.Name))
        {
            architecture = _is64Bit ? "AMD64" : "X86";
        }

        var assemblyName = identity.FullName + ", processorArchitecture=" + architecture;

        // .NET Core only works with absolute path, so first load fusion.dll manually
        var path = _fusionWrapper.QueryAssemblyPath(assemblyName);
        return path != null ? new AssemblyResolverResult(AssemblySource.Gac, path) : null;
    }

    public IEnumerable<string> FindFusionPaths()
    {
        var basePath = Path.Combine(_environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.NET");
        var frameworkDirectory = _environment.Is64BitOperatingSystem ? "Framework64" : "Framework";
        foreach (var version in FrameworkVersions)
        {
            var fusionPath = Path.Combine(basePath, frameworkDirectory, version, "fusion.dll");
            if (_fileSystem.File.Exists(fusionPath))
            {
                yield return fusionPath;
            }
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

        _fusionWrapper.Dispose();
    }
}

internal interface IFusionWrapper : IDisposable
{
    void LoadFusion(IEnumerable<string> fusionPaths);
    string? QueryAssemblyPath(string assemblyName);
}

[ExcludeFromCodeCoverage]
internal class FusionWrapper : IFusionWrapper
{
    private IntPtr _fusionLibrary;

    public string? QueryAssemblyPath(string assemblyName)
    {
        if (_fusionLibrary == IntPtr.Zero)
        {
            throw new FileLoadException("Call IFusionWrapper.Load(paths) before querying for assemblies.");
        }

        const int bufferSize = 1024;
        var assemblyInfo = new AssemblyInfo
        {
            cchBuf = bufferSize,
            pszCurrentAssemblyPathBuf = new string('\0', bufferSize)
        };

        var hr = CreateAssemblyCache(out var assemblyCache, 0);
        if (hr >= 0)
        {
            hr = assemblyCache.QueryAssemblyInfo(0, assemblyName, ref assemblyInfo);
        }

        if (hr == -2147024894)
        {
            return null;
        }

        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        return assemblyInfo.pszCurrentAssemblyPathBuf;
    }

    public void LoadFusion(IEnumerable<string> fusionPaths)
    {
        if (_fusionLibrary != IntPtr.Zero)
        {
            return;
        }

        foreach (var path in fusionPaths)
        {
            try
            {
                _fusionLibrary = NativeLibrary.Load(path);
                break;
            }
            catch
            {
                throw new FileLoadException($"Could not load fusion.dll ({path})");
            }
        }

        if (_fusionLibrary != IntPtr.Zero)
        {
            return;
        }

        throw new FileNotFoundException("Could not find fusion.dll");
    }


    [DllImport("fusion.dll")]
    private static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing || _fusionLibrary == IntPtr.Zero)
        {
            return;
        }

        NativeLibrary.Free(_fusionLibrary);
        _fusionLibrary = IntPtr.Zero;
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
    private interface IAssemblyCache
    {
        void UninstallAssembly();

        [PreserveSig]
        int QueryAssemblyInfo(
            int flags,
            [MarshalAs(UnmanagedType.LPWStr)] string assemblyName,
            ref AssemblyInfo assemblyInfo);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AssemblyInfo
    {
        private readonly int cbAssemblyInfo;
        private readonly int dwAssemblyFlags;
        private readonly long uliAssemblySizeInKB;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszCurrentAssemblyPathBuf;

        public int cchBuf;
    }
}