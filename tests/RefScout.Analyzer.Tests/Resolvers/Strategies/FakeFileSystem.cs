using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies;

internal class FakeFileSystem : MockFileSystem
{
    private static readonly string[] Files =
    {
        // .NET Framework Compact 3.5
        @"C:\Program Files (x86)\Microsoft.NET\SDK\CompactFramework\v3.5\WindowsCE\mscorlib.dll",
        @"C:\Program Files\Microsoft.NET\SDK\CompactFramework\v3.5\WindowsCE\mscorlib.dll",

        // .NET Framework mscorlib
        @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll",
        @"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\mscorlib.dll",
        @"C:\Windows\Microsoft.NET\Framework\v1.1.4322\mscorlib.dll",

        // .NET Framework GAC
        @"C:\Windows\Microsoft.NET\assembly\GAC_64\System.Data\v4.0_4.0.0.0__b77a5c561934e089\System.Data.dll",
        @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel.Http\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.ServiceModel.Http.dll",
        @"C:\Windows\assembly\GAC\Microsoft.DirectX\1.0.2902.0__31bf3856ad364e35\Microsoft.DirectX.dll",

        // Fusion
        @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\fusion.dll",
        @"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\fusion.dll",

        // .NET Core shared
        @"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\5.0.0\PresentationCore.dll",
        @"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\5.0.0\WindowsBase.dll",
        @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.0\WindowsBase.dll",
        @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.0\System.Data.dll",
        @"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\3.1.0\WindowsBase.dll",
        @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.0\WindowsBase.dll",
        @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\6.0.0-preview.7.21377.19\WindowsBase.dll",
        @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\beepboopinvalidversion\WindowsBase.dll",

        // Silverlight
        @"C:\Program Files (x86)\Microsoft Silverlight\5.1.50918.0\System.Windows.dll",
        @"C:\Program Files\Microsoft Silverlight\4.1.10329.0\System.ServiceModel.dll",

        // Windows Metadata
        @"C:\Program Files (x86)\Windows Kits\10\References\10.0.19041.0\Windows.System.Profile.ProfileRetailInfoContract\1.0.0.0\Windows.System.Profile.ProfileRetailInfoContract.winmd",
        @"C:\Windows\System32\WinMetadata\Windows.Globalization.winmd",
        @"C:\Windows\System32\WinMetadata\Windows.ApplicationModel.winmd",

        // NuGet packages
        @"C:\Users\Joery\.nuget\packages\system.io\4.3.0\lib\net462\System.IO.dll",
        @"C:\Users\Joery\.nuget\packages\system.bratwurst\4.3.0\lib\net462\System.Bratwurst.exe",

        // Mono
        @"C:\Program Files\Mono\lib\mono\4.5\mscorlib.dll",
        @"C:\Program Files\Mono\lib\mono\4.5\System.IO.Compression.dll",
        @"C:\Program Files\Mono\lib\mono\gac\Newtonsoft.Json\12.0.0.0__30ad4fe6b2a6aeed\Newtonsoft.Json.dll",

        // Project folder
        @"C:\project\Application.exe",
        @"C:\project\Application.dll",
        @"C:\project\en-US\Application.exe",
        @"C:\project\en-US\Application.dll",
        @"C:\project\Executable.exe",
        @"C:\project\api-test-xd.dll",
        @"C:\project\probe\Probe.dll",
        @"C:\project\probe\Application.dll"
    };

    public FakeFileSystem() : base(Files.Select(a => (a, new MockFileData("empty")))
        .ToDictionary(a => a.a, b => b.Item2)) { }
}