using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using RefScout.Analyzer;
using RefScout.Core.Logging;
using RefScout.Wpf.Services;
using RefScout.Wpf.Views;

namespace RefScout.Wpf.ViewModels;

internal class MainWindowViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IServiceProvider _serviceProvider;
    private string? _errorMessage;
    private bool _isAnalyzing;
    private bool _isDragging;
    private IAnalyzerResult? _result;

    private bool _showWelcomeScreen;
    private IReadOnlyList<Assembly> _treeAssemblies;

    private CancellationTokenSource? _cancellationTokenSource;
    private string? _currentPath;

    public MainWindowViewModel(ISettingsService settingsService, IServiceProvider serviceProvider)
    {
        _settingsService = settingsService;
        _serviceProvider = serviceProvider;

        _treeAssemblies = Array.Empty<Assembly>();

        AnalyzeAssembly = new AsyncRelayCommand<string>(DoAnalyzeAssemblyAsync);
        CancelAnalyzeAssembly = new RelayCommand(DoCancelAnalyzeAssembly);

        ShowSettingsDialog = new RelayCommand(DoShowSettingsDialog);
        AnalyzeAgain = new AsyncRelayCommand(DoAnalyzeAgain);

        ShowFileChooser = new AsyncRelayCommand(DoShowFileChooser);
        ShowWelcomeScreen = true;
    }

    public bool ShowWelcomeScreen
    {
        get => _showWelcomeScreen;
        set => SetProperty(ref _showWelcomeScreen, value);
    }

    public bool IsDragging
    {
        get => _isDragging;
        set => SetProperty(ref _isDragging, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsAnalyzing
    {
        get => _isAnalyzing;
        set => SetProperty(ref _isAnalyzing, value);
    }

    public IAnalyzerResult? Result
    {
        get => _result;
        set => SetProperty(ref _result, value);
    }

    public IReadOnlyList<Assembly> TreeAssemblies
    {
        get => _treeAssemblies;
        private set => SetProperty(ref _treeAssemblies, value);
    }

    public AsyncRelayCommand<string> AnalyzeAssembly { get; }
    public RelayCommand CancelAnalyzeAssembly { get; }

    public RelayCommand ShowSettingsDialog { get; }
    public AsyncRelayCommand AnalyzeAgain { get; }
    public AsyncRelayCommand ShowFileChooser { get; }

    private void DoShowSettingsDialog()
    {
        var settingsDialog = _serviceProvider.GetRequiredService<SettingsWindow>();
        settingsDialog.Owner = Application.Current.MainWindow;
        settingsDialog.ShowDialog();
    }

    private async Task DoAnalyzeAgain()
    {
        await DoAnalyzeAssemblyAsync(_currentPath);
    }

    private async Task DoShowFileChooser()
    {
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".png",
            Filter = "Assembly Files (*.dll, *.exe)|*.dll;*.exe"
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            await DoAnalyzeAssemblyAsync(dialog.FileName).ConfigureAwait(false);
        }
    }

    private async Task DoAnalyzeAssemblyAsync(string? path)
    {
        if (path == null)
        {
            return;
        }

        _currentPath = path;
        IsAnalyzing = true;
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            Result = await Task.Run(() => ReferenceAnalyzer.Run(path, new AnalyzerOptions
            {
                AnalyzeMode = _settingsService.Settings.AnalyzeMode,
                SystemVersionMode = _settingsService.Settings.SystemVersionMode
            }, _cancellationTokenSource.Token));

            TreeAssemblies = Result.Assemblies.Take(1).ToList();

            ShowWelcomeScreen = false;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Could not analyze assembly");
            ShowWelcomeScreen = true;
            ErrorMessage = e.Message;
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    private void DoCancelAnalyzeAssembly()
    {
        _cancellationTokenSource?.Cancel();
    }
}