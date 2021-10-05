using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using RefScout.Analyzer;
using RefScout.Core.Logging;
using RefScout.Visualizers;
using RefScout.Visualizers.Dot;
using RefScout.Wpf.Helpers;
using RefScout.Wpf.Models;
using RefScout.Wpf.Services;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace RefScout.Wpf.ViewModels;

internal class GraphContainerViewModel : ObservableObject
{
    private const string DownloadUrl = "https://graphviz.org/download/#windows";

    private readonly IContextService _context;
    private readonly ISettingsService _settings;

    private IAnalyzerResult? _analyzerResult;
    private string? _errorMessage;
    private bool _graphVizNotInstalled;
    private string? _loadingMessage;
    private string? _vectorPath;

    public GraphContainerViewModel(ISettingsService settings, IContextService context)
    {
        _settings = settings;
        _context = context;

        GraphVizNotInstalled = !GraphVizHelper.IsGraphVizInstalled();
        DownloadGraphViz = new RelayCommand(DoDownloadGraphViz);
        InstallForMe = new AsyncRelayCommand(DoInstallForMeAsync);
        TryAgain = new AsyncRelayCommand(DoTryAgainAsync);
        ExportGraph = new RelayCommand(DoExportGraph);
        ToggleGraphDirection = new AsyncRelayCommand(DoToggleGraphDirectionAsync);
        ToggleShowTargetFramework = new AsyncRelayCommand(DoToggleShowDotNetFrameworkAsync);
        VisualizeModeChanged = new AsyncRelayCommand(DoVisualizeModeChangedAsync);

        VisualizeModes = new List<ComboBoxEntry<VisualizeMode>>
        {
            new(VisualizeMode.All, "All assemblies"),
            new(VisualizeMode.Conflicts, "Only conflicts"),
            new(VisualizeMode.App, "Application assemblies")
        };
    }

    public AppSettings Settings => _settings.Settings;

    public RelayCommand DownloadGraphViz { get; }
    public AsyncRelayCommand InstallForMe { get; }
    public AsyncRelayCommand TryAgain { get; }
    public RelayCommand ExportGraph { get; }
    public AsyncRelayCommand ToggleGraphDirection { get; }
    public AsyncRelayCommand ToggleShowTargetFramework { get; }
    public AsyncRelayCommand VisualizeModeChanged { get; }

    public IAnalyzerResult? AnalyzerResult
    {
        get => _analyzerResult;
        set => SetProperty(ref _analyzerResult, value);
    }

    public string? LoadingMessage
    {
        get => _loadingMessage;
        set => SetProperty(ref _loadingMessage, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool GraphVizNotInstalled
    {
        get => _graphVizNotInstalled;
        set => SetProperty(ref _graphVizNotInstalled, value);
    }

    public string? VectorPath
    {
        get => _vectorPath;
        set => SetProperty(ref _vectorPath, value);
    }

    public List<ComboBoxEntry<VisualizeMode>> VisualizeModes { get; }

    public ComboBoxEntry<VisualizeMode> SelectedVisualizeMode
    {
        get => VisualizeModes.Single(v => v.Value == _settings.Settings.VisualizeMode);
        set => _settings.Settings.VisualizeMode = value.Value;
    }

    private void DoExportGraph()
    {
        if (VectorPath == null)
        {
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Title = "Export graph as image",
            Filter = "PNG Image|*.png|SVG Image|*.svg"
        };

        if (saveFileDialog.ShowDialog() == false)
        {
            return;
        }

        switch (Path.GetExtension(saveFileDialog.FileName))
        {
            case ".png":
                // TODO: this still doesn't work, waiting for SharpVectors fix...
                var settings = new WpfDrawingSettings
                {
                    IgnoreRootViewbox = false,
                    PixelWidth = 936,
                    PixelHeight = 2760
                };

                var converter = new ImageSvgConverterEx(settings)
                {
                    EncoderType = ImageEncoderType.PngBitmap
                };
                converter.Convert(VectorPath, saveFileDialog.FileName);
                break;
            case ".svg":
                File.Copy(VectorPath, saveFileDialog.FileName, true);
                break;
        }

        MessageBox.Show("Successfully exported graph to image");
        //_toast.ShowToast(ToastType.Success, "Successfully exported graph to image");
    }

    private async Task DoTryAgainAsync()
    {
        GraphVizNotInstalled = !GraphVizHelper.IsGraphVizInstalled();
        await RenderAnalyzerResultAsync();
    }

    private static void DoDownloadGraphViz()
    {
        ProcessHelper.LaunchBrowser(DownloadUrl);
    }

    private async Task DoInstallForMeAsync()
    {
        try
        {
            await GraphVizHelper.DownloadAndInstallGraphViz(new Progress<string>(message =>
                LoadingMessage = $"Installing GraphViz: {message}"));
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to automatically install GraphViz");
            GraphVizNotInstalled = true;
            LoadingMessage = null;
            return;
        }

        await DoTryAgainAsync();
    }

    private async Task DoToggleGraphDirectionAsync()
    {
        Settings.GraphTopBottom = !Settings.GraphTopBottom;
        await RenderAnalyzerResultAsync();
    }

    private async Task DoToggleShowDotNetFrameworkAsync()
    {
        Settings.GraphShowTargetFramework = !Settings.GraphShowTargetFramework;
        await RenderAnalyzerResultAsync();
    }

    private async Task DoVisualizeModeChangedAsync()
    {
        await RenderAnalyzerResultAsync();
    }

    public async Task RenderAnalyzerResultAsync()
    {
        if (AnalyzerResult == null)
        {
            return;
        }

        LoadingMessage = "Generating graph";

        var previousVectorPath = VectorPath;
        string? tempGraphPath = null;
        try
        {
            tempGraphPath = Path.Combine(GraphVizHelper.TempGraphVizFolder, Path.GetRandomFileName());
            var visualizer = new DotConflictVisualizer();
            await Task.Run(() => visualizer.Visualize(AnalyzerResult, _settings.Settings.VisualizeMode,
                new DotConflictVisualizerOptions(tempGraphPath)
                {
                    ShowTargetFramework = _settings.Settings.GraphShowTargetFramework,
                    DarkTheme = _settings.Settings.DarkTheme,
                    Direction = Settings.GraphTopBottom ? GraphDirection.Tb : GraphDirection.Lr
                }));

            VectorPath = await GraphVizHelper.ConvertGraphVizToImage(tempGraphPath);
            LoadingMessage = "Rendering graph";
        }
        catch (Exception e)
        {
            Logger.Error(e, "Could not visualize analyzer result");

            if (e is FileNotFoundException)
            {
                GraphVizNotInstalled = true;
            }
            else
            {
                ErrorMessage = e.Message;
            }

            LoadingMessage = null;
        }
        finally
        {
            CleanUpTemporaryFiles(tempGraphPath, previousVectorPath);
        }
    }

    public void OpenDetailsWindow(string id)
    {
        var assembly = AnalyzerResult?.Assemblies.SingleOrDefault(a => a.Id == id);
        if (assembly != null)
        {
            _context.ShowDetailsWindow(assembly);
        }
    }

    private static void CleanUpTemporaryFiles(string? tempGraphPath, string? previousVectorPath)
    {
        if (!string.IsNullOrEmpty(tempGraphPath) && File.Exists(tempGraphPath))
        {
            File.Delete(tempGraphPath);
        }

        if (!string.IsNullOrEmpty(previousVectorPath) && File.Exists(previousVectorPath))
        {
            File.Delete(previousVectorPath);
        }
    }
}