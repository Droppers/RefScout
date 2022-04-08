using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Mono.Cecil;

namespace RefScout.Analyzer.Readers.Cecil;

internal class CecilMetadataReader : IMetadataReader
{
    private readonly AssemblyDefinition _definition;

    public CecilMetadataReader(AssemblyDefinition definition)
    {
        _definition = definition;
    }

    public AssemblyKind ReadKind()
    {
        var kind = _definition.MainModule.Kind switch
        {
            ModuleKind.Windows => AssemblyKind.Windows,
            ModuleKind.Console => AssemblyKind.Console,
            _ => AssemblyKind.Dll
        };

        if (kind != AssemblyKind.Dll)
        {
            return kind;
        }

        return _definition.MainModule.AssemblyReferences.Any(reference =>
            reference.Name.StartsWith("Microsoft.AspNetCore.", StringComparison.OrdinalIgnoreCase))
            ? AssemblyKind.Web
            : kind;
    }

    public (string architectureString, ProcessorArchitecture architecture, bool is64Bit) ReadProcessorArchitecture()
    {
        var architecture = (int)_definition.MainModule.Architecture;

        // Temporary fix until Mono.Cecil is updated
        // See: https://github.com/dotnet/runtime/issues/36364
        // And: https://github.com/jbevain/cecil/issues/797
        if (architecture > (int)TargetArchitecture.AMD64)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                architecture ^= 0x4644;
            }
            else
            {
                architecture ^= 0x7B79;
            }
        }

        var characteristics = _definition.MainModule.Characteristics;
        var corFlags = _definition.MainModule.Attributes;
        switch ((TargetArchitecture)architecture)
        {
            case TargetArchitecture.I386:
                if ((corFlags & ModuleAttributes.Preferred32Bit) != 0)
                {
                    return ("Any CPU (32-bit preferred)", ProcessorArchitecture.Cil, false);
                }

                if ((corFlags & ModuleAttributes.Required32Bit) != 0)
                {
                    return ("x86", ProcessorArchitecture.X86, false);
                }

                if ((corFlags & ModuleAttributes.ILOnly) == 0 &&
                    (characteristics & ModuleCharacteristics.NXCompat) != 0)
                {
                    return ("x86", ProcessorArchitecture.X86, false);
                }

                return ("Any CPU", ProcessorArchitecture.Cil, true);
            case TargetArchitecture.AMD64:
                return ("x64", ProcessorArchitecture.Amd64, true);
            case TargetArchitecture.IA64:
                return ("Itanium", ProcessorArchitecture.Ia64, true);
            case TargetArchitecture.ARMv7:
            case TargetArchitecture.ARM:
                return ("ARM", ProcessorArchitecture.Arm, false);
            case TargetArchitecture.ARM64:
                return ("ARM64", ProcessorArchitecture.Arm64, true);
            default:
                throw new Exception($"Target architecture {architecture} of '{_definition.FullName}' is not supported");
        }
    }

    public IEnumerable<AssemblyIdentity> ReadReferences()
        => _definition.MainModule.AssemblyReferences.Select(CecilAssemblyReader.MapNameToIdentity);

    public AssemblySourceLanguage ReadSourceLanguage()
        => LanguageDetector.DetectLanguageFromAssembly(_definition);

    public TargetFramework ReadTargetFramework()
    {
        var targetFramework = GetCustomAttributeValue<TargetFrameworkAttribute>();
        var productAttribute = GetCustomAttributeValue<AssemblyProductAttribute>();

        // Why did I even write this for Silverlight...
        var isSilverlight = productAttribute == "Microsoft® Silverlight";
        if (isSilverlight)
        {
            var versionAttribute = GetCustomAttributeValue<AssemblyVersionAttribute>();
            if (Version.TryParse(versionAttribute, out var parsedVersion) || _definition.Name.Version.Major >= 5)
            {
                return new TargetFramework(NetRuntime.Silverlight, parsedVersion ?? _definition.Name.Version);
            }
        }

        if (targetFramework != null)
        {
            return TargetFramework.Parse(targetFramework);
        }

        foreach (var reference in _definition.MainModule.AssemblyReferences)
        {
            switch (reference.Name)
            {
                case "netstandard":
                    return new TargetFramework(NetRuntime.Standard,
                        new Version(reference.Version.Major, reference.Version.Minor));
                case "System.Runtime" when reference.Version >= new Version(4, 2, 0):
                    var version = "2.0";
                    if (reference.Version >= new Version(6, 0))
                    {
                        version = "6.0";
                    }
                    else if (reference.Version >= new Version(5, 0))
                    {
                        version = "5.0";
                    }
                    else if (reference.Version >= new Version(4, 2, 1))
                    {
                        version = "3.0";
                    }
                    else if (reference.Version >= new Version(4, 2, 2))
                    {
                        version = "3.1";
                    }

                    return new TargetFramework(NetRuntime.Core, new Version(version));
                case "mscorlib" when reference.Version.Major < 5:
                    return new TargetFramework(NetRuntime.Framework, reference.Version);
                case "mscorlib" when reference.Version.Major >= 5:
                    return new TargetFramework(NetRuntime.Silverlight, reference.Version);
            }
        }

        return new TargetFramework(NetRuntime.Framework,
            new Version(_definition.MainModule.Runtime switch
            {
                TargetRuntime.Net_1_0 => "1.0",
                TargetRuntime.Net_1_1 => "1.1",
                TargetRuntime.Net_2_0 => "2.0",
                TargetRuntime.Net_4_0 => "4.0",
                _ => throw new Exception(
                    $"TargetRuntime {_definition.MainModule.Runtime} of '{_definition.FullName}' is not supported")
            }));
    }

    private string? GetCustomAttributeValue<T>()
    {
        return _definition
            .CustomAttributes
            .SingleOrDefault(a => a.AttributeType.FullName == typeof(T).FullName)
            ?.ConstructorArguments.FirstOrDefault().Value?.ToString();
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

        _definition.Dispose();
    }
}