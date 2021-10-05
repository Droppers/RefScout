using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Config.Framework;
using Xunit;

namespace RefScout.Analyzer.Tests.Config.Framework;

public class FrameworkConfigParserTests
{
    private readonly IConfigParser<FrameworkConfig> _parser;

    public FrameworkConfigParserTests()
    {
        // Use real file system, since it relies on reading and parsing the actual test files
        _parser = new FrameworkConfigParser(new FileSystem());
    }

    [Fact]
    public void ParseFile_NonExistentConfig()
    {
        var path = GetPath("NonExistent.config");
        var config = _parser.ParseFile(path, path);

        Assert.NotNull(config);
    }

    [Fact]
    public void ParseFile_ConfigWithoutBindings()
    {
        var path = GetPath("NoBindings.config");
        var config = _parser.ParseFile(path, path);

        Assert.NotNull(config);
        Assert.False(config.ErrorReport.HasErrors);
        Assert.Empty(config.ProbeFolders);
        Assert.Empty(config.CodeBases);
        Assert.Empty(config.BindingRedirects);
    }

    [Fact]
    public void ParseFile_LargeConfigFromVisualStudio()
    {
        var path = GetPath("HasEverything.config");
        var config = _parser.ParseFile(path, path);

        var expectedProbeFolders = new[]
        {
            "PublicAssemblies",
            "PrivateAssemblies",
            @"CommonExtensions\Microsoft\TestWindow",
            @"CommonExtensions\Platform\Debugger",
            @"PrivateAssemblies\DataCollectors",
            @"PrivateAssemblies\DataCollectors\x86"
        };
        Assert.NotNull(config);
        Assert.False(config.ErrorReport.HasErrors);
        Assert.Equal(expectedProbeFolders, config.ProbeFolders);
        Assert.Equal(51, config.CodeBases.Count);
        Assert.Equal(318, config.BindingRedirects.Count);

        var bindingRedirect = config.BindingRedirects.Where(x =>
            x.Identity.Name == "Microsoft.IdentityModel.Clients.ActiveDirectory").ToList();
        var codeBase =
            config.CodeBases.Where(x => x.Identity.Name == "Microsoft.IdentityModel.Clients.ActiveDirectory")
                .ToList();
        Assert.Equal(2, bindingRedirect.Count);
        Assert.Equal(2, codeBase.Count);

        Assert.Equal(new Version(5, 2, 9, 0), bindingRedirect.Last().NewVersion);
        Assert.Equal(@"PrivateAssemblies\AdalV5\Microsoft.IdentityModel.Clients.ActiveDirectory.dll",
            codeBase.Last().RelativeHref);
    }

    [Fact]
    public void ParseFile_BrokenConfig()
    {
        var path = GetPath("Broken.config");
        var config = _parser.ParseFile(path, path);

        Assert.NotNull(config);
        Assert.Equal(11, config.ErrorReport.Errors.Count);
    }

    private static string GetPath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "TestFiles", "Configs", name);
}