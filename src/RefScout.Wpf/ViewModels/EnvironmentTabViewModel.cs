using System.Linq;
using System.Text;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using RefScout.Analyzer;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Context;

namespace RefScout.Wpf.ViewModels;

internal class EnvironmentTabViewModel : ObservableObject
{
    private IAnalyzerResult? _analyzerResult;
    private string? _configErrorMessage;
    private EnvironmentInfo? _environment;

    private string? _coreEnvironmentDetails;
    private string? _frameworkEnvironmentDetails;

    public IAnalyzerResult? AnalyzerResult
    {
        get => _analyzerResult;
        set => SetProperty(ref _analyzerResult, value);
    }

    public EnvironmentInfo? Environment
    {
        get => _environment;
        set => SetProperty(ref _environment, value);
    }

    public string? ConfigErrorMessage
    {
        get => _configErrorMessage;
        set => SetProperty(ref _configErrorMessage, value);
    }

    public string? CoreEnvironmentDetails
    {
        get => _coreEnvironmentDetails;
        set => SetProperty(ref _coreEnvironmentDetails, value);
    }

    public string? FrameworkEnvironmentDetails
    {
        get => _frameworkEnvironmentDetails;
        set => SetProperty(ref _frameworkEnvironmentDetails, value);
    }

    public void OnNewAnalyzerResult(IAnalyzerResult? analyzerResult)
    {
        AnalyzerResult = analyzerResult;
        Environment = analyzerResult?.EnvironmentInfo;

        CoreEnvironmentDetails = BuildCoreEnvironmentDetails(analyzerResult);
        FrameworkEnvironmentDetails = BuildFrameworkEnvironmentDetails(analyzerResult);

        if (analyzerResult is FrameworkAnalyzerResult frameworkResult &&
            frameworkResult.Config.ErrorReport.HasErrors)
        {
            ConfigErrorMessage = frameworkResult.Config.ErrorReport.ToString();
        }
        else
        {
            ConfigErrorMessage = null;
        }
    }

    private static string BuildCoreEnvironmentDetails(IAnalyzerResult? result)
    {
        if (result is null)
        {
            return "Unknown";
        }

        var sb = new StringBuilder();
        if (result is CoreAnalyzerResult coreAnalyzerResult)
        {
            sb.Append("  Used runtime version: ");
            if (coreAnalyzerResult.Config.SelfContained)
            {
                sb.AppendLine("self-contained application");
            }
            else if (coreAnalyzerResult.Runtime != null)
            {
                sb.Append(coreAnalyzerResult.Runtime.VersionName);
                sb.AppendLine($" (policy: {coreAnalyzerResult.Config.RollForward})");
            }
            else
            {
                sb.AppendLine("Could not find appropriate runtime version.");
            }
        }

        if (result.EnvironmentInfo.Core != null)
        {
            var x64Runtimes = result.EnvironmentInfo.Core.Runtimes.Where(x => x.Is64Bit).ToList();
            var x86Runtimes = result.EnvironmentInfo.Core.Runtimes.Where(x => !x.Is64Bit).ToList();
            sb.Append("  Installed versions: ");
            sb.AppendLine(x64Runtimes.Count > 0
                ? string.Join(", ", x64Runtimes.Select(s => s.VersionName))
                : "None installed.");

            sb.Append("  Installed versions (x86): ");
            sb.AppendLine(x86Runtimes.Count > 0
                ? string.Join(", ", x86Runtimes.Select(s => s.VersionName))
                : "None installed.");
        }

        return sb.ToString();
    }

    private static string BuildFrameworkEnvironmentDetails(IAnalyzerResult? result)
    {
        if (result?.EnvironmentInfo.Framework == null)
        {
            return "Unknown";
        }


        var sb = new StringBuilder();
        foreach (var (framework, version, servicePack) in result.EnvironmentInfo.Framework.Runtimes)
        {
            sb.AppendLine($"  {framework}: {version} {(servicePack > 0 ? $"(service pack {servicePack})" : "")}");
        }

        return sb.ToString();
    }
}