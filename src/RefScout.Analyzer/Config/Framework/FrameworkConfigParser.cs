using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Config.Framework;

// This is not beautiful but it works and I am not a fan of parsing XML
internal class FrameworkConfigParser : IConfigParser<FrameworkConfig>
{
    private static readonly string[] ValidRuntimeVersions =
        { "v1.0.3705", "v1.1.4322", "v2.0.50727", "v4.0", "v4.0.30319" };

    private static readonly string[] ValidSkuValues =
    {
        ".NETFramework,Version=v4.0",
        ".NETFramework,Version=v4.0,Profile=Client",
        ".NETFramework,Version=v4.0.1",
        ".NETFramework,Version=v4.0.1,Profile=Client",
        ".NETFramework,Version=v4.0.2",
        ".NETFramework,Version=v4.0.2,Profile=Client",
        ".NETFramework,Version=v4.0.3",
        ".NETFramework,Version=v4.0.3,Profile=Client",
        ".NETFramework,Version=v4.5",
        ".NETFramework,Version=v4.5.1",
        ".NETFramework,Version=v4.5.2",
        ".NETFramework,Version=v4.6",
        ".NETFramework,Version=v4.6.1",
        ".NETFramework,Version=v4.6.2",
        ".NETFramework,Version=v4.7",
        ".NETFramework,Version=v4.7.1",
        ".NETFramework,Version=v4.7.2",
        ".NETFramework,Version=v4.8"
    };

    private readonly IFileSystem _fileSystem;

    public FrameworkConfigParser(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public FrameworkConfig ParseFile(string assemblyFileName, string? configFileName)
    {
        if (configFileName is null || !_fileSystem.File.Exists(configFileName))
        {
            return new FrameworkConfig();
        }

        var isMachineConfig = Path.GetFileName(configFileName) == "machine.config";

        using var file =
            _fileSystem.File.OpenRead(configFileName);
        var document = XDocument.Load(file, LoadOptions.SetLineInfo);

        var namespaceManager = new XmlNamespaceManager(new NameTable());
        namespaceManager.AddNamespace("bind", "urn:schemas-microsoft-com:asm.v1");

        var supportedRuntimeNodes = document.XPathSelectElements("//startup/supportedRuntime");
        var probingNode = document.XPathSelectElement("//bind:probing", namespaceManager);
        var nodes = document.XPathSelectElements("//bind:dependentAssembly", namespaceManager).ToList();


        var errorReport = new FrameworkConfigErrorReport();
        var probeFolders = probingNode != null ? ParseProbeFolders(probingNode) : Array.Empty<string>();
        var supportedRuntimes = ParseSupportedRuntimes(supportedRuntimeNodes, errorReport).ToList();
        var (codeBases, bindingRedirects) =
            ParseBindingRelated(nodes, Path.GetDirectoryName(assemblyFileName), isMachineConfig, errorReport);

        return new FrameworkConfig
        {
            SupportedRuntimes = supportedRuntimes,
            ProbeFolders = probeFolders,
            BindingRedirects = bindingRedirects,
            CodeBases = codeBases,
            ErrorReport = errorReport
        };
    }

    private static IReadOnlyList<string> ParseProbeFolders(
        XElement node)
    {
        if (!node.HasAttributes)
        {
            return Array.Empty<string>();
        }

        var str = node.Attribute("privatePath")?.Value;

        return !string.IsNullOrEmpty(str)
            ? str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            : Array.Empty<string>();
    }

    private static IEnumerable<TargetFramework> ParseSupportedRuntimes(
        IEnumerable<XElement> elements,
        FrameworkConfigErrorReport errorReport)
    {
        foreach (var element in elements)
        {
            errorReport.ReportIfAllAttributesMissing(element, new[] { "version", "sku" });

            var version = element.Attribute("version")?.Value;
            var sku = element.Attribute("sku")?.Value;

            TargetFramework? supportedTarget = null;
            if (version != null && !ValidRuntimeVersions.Contains(version))
            {
                var versions = string.Join(", ", ValidRuntimeVersions);
                errorReport.Report(element, $"Attribute 'version' expects one of: '{versions}'.");
            }
            else if (version != null)
            {
                supportedTarget = new TargetFramework(NetRuntime.Framework,
                    Version.Parse(version.Trim('v')).ToMajorMinor());
            }

            if (version == null && sku != null)
            {
                // Not displaying all SKUs, list is too long
                errorReport.Report(element,
                    "Attribute 'version' with value 'v4.0' required when using attribute 'sku'.");
            }

            if (sku != null && !ValidSkuValues.Contains(sku))
            {
                // Not displaying all SKUs, list is too long
                errorReport.Report(element, "Invalid value for attribute 'sku'.");
            }
            else if (sku != null)
            {
                supportedTarget = TargetFramework.Parse(sku);
            }

            if (supportedTarget != null)
            {
                yield return supportedTarget;
            }
        }
    }

    private static (IReadOnlyList<CodeBase> codeBase, IReadOnlyList<BindingRedirect>) ParseBindingRelated(
        IEnumerable<XElement> nodes,
        string? baseDirectory,
        bool isMachineConfig,
        FrameworkConfigErrorReport errorReport)
    {
        var codeBases = new List<CodeBase>();
        var bindingRedirects = new List<BindingRedirect>();
        foreach (var node in nodes)
        {
            var identity = ParseIdentity(node, isMachineConfig, errorReport);
            if (identity == null)
            {
                continue;
            }

            var codeBase = ParseCodeBase(node, identity, baseDirectory ?? "", errorReport);
            if (codeBase != null)
            {
                codeBases.Add(codeBase);
            }

            var bindingRedirect = ParseBindingRedirect(node, identity, errorReport);
            if (bindingRedirect != null)
            {
                bindingRedirects.Add(bindingRedirect);
            }
        }

        return (codeBases, bindingRedirects);
    }

    private static BindingIdentity? ParseIdentity(
        XElement dependentAssembly,
        bool isMachineConfig,
        FrameworkConfigErrorReport errorReport)
    {
        var identityNode = dependentAssembly.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "assemblyIdentity");
        if (identityNode == null)
        {
            errorReport.Report(dependentAssembly, "Required element <assemblyIdentity /> is missing.");
            return null;
        }

        errorReport.ReportIfAttributeMissing(identityNode, "name");
        errorReport.ReportIfAttributeMissing(identityNode, "publicKeyToken");

        var hexString = identityNode.Attribute("publicKeyToken")?.Value;
        var token = PublicKeyToken.Empty;
        if (hexString != null)
        {
            if (PublicKeyToken.TryParse(hexString, out var parsedToken))
            {
                token = parsedToken;
            }
            else
            {
                errorReport.Report(identityNode, "Invalid value for attribute 'publicKeyToken'.");
            }
        }

        var identityName = identityNode.Attribute("name")?.Value;
        if (identityName != null)
        {
            return new BindingIdentity(identityName,
                identityNode.Attribute("culture")?.Value ?? AssemblyIdentity.CultureNeutral,
                token, isMachineConfig);
        }

        return null;
    }

    private static CodeBase? ParseCodeBase(
        XElement node,
        BindingIdentity identity,
        string basePath,
        FrameworkConfigErrorReport errorReport)
    {
        var codeBaseNode = node.Descendants().FirstOrDefault(x => x.Name.LocalName == "codeBase");
        if (codeBaseNode == null)
        {
            return null;
        }

        // Version attribute is ignored for not strong-named assemblies
        if (identity.IsStrongNamed)
        {
            errorReport.ReportIfAttributeMissing(codeBaseNode, "version");
        }

        errorReport.ReportIfAttributeMissing(codeBaseNode, "href");

        var version = codeBaseNode.Attribute("version")?.Value;
        var href = codeBaseNode.Attribute("href")?.Value;

        if (href == null || version == null)
        {
            return null;
        }

        // TODO: Add better support for parsing href URI's
        var absoluteHref = href.StartsWith(@"file:///")
            ? href.Replace(@"file:///", "")
            : Path.Combine(basePath, href);
        return new CodeBase(identity, new Version(version), href, absoluteHref);
    }

    private static BindingRedirect? ParseBindingRedirect(
        XElement node,
        BindingIdentity identity,
        FrameworkConfigErrorReport errorReport)
    {
        var redirectNode = node.Descendants().FirstOrDefault(x => x.Name.LocalName == "bindingRedirect");
        if (redirectNode == null)
        {
            return null;
        }

        errorReport.ReportIfAttributeMissing(redirectNode, "oldVersion");
        errorReport.ReportIfAttributeMissing(redirectNode, "newVersion");

        var oldVersion = redirectNode.Attribute("oldVersion")?.Value;
        var newVersion = redirectNode.Attribute("newVersion")?.Value;

        // TODO: Add handling for incorrect version format
        if (oldVersion != null && newVersion != null)
        {
            var oldVersions = oldVersion.Split('-').Select(v => new Version(v))
                .ToList();

            return new BindingRedirect(identity,
                new Version(newVersion),
                oldVersions[0],
                oldVersions.Last());
        }

        return null;
    }
}