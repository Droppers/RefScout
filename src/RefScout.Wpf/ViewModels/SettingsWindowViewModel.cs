using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using RefScout.Analyzer;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Wpf.Helpers;
using RefScout.Wpf.Models;
using RefScout.Wpf.Services;

namespace RefScout.Wpf.ViewModels;

internal class SettingsWindowViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly ILoggingService _logging;

    public SettingsWindowViewModel(ISettingsService settings, ILoggingService logging)
    {
        _settings = settings;
        _logging = logging;

        AnalyzerModes = new List<ComboBoxEntry<AnalyzeMode>>
        {
            new(AnalyzeMode.AppDirectSystem, "Application and direct system assemblies"),
            new(AnalyzeMode.App, "Application assemblies only"),
            new(AnalyzeMode.All, "All assemblies")
        };

        SystemVersionModes = new List<ComboBoxEntry<VersionCompatibilityMode>>
        {
            new(VersionCompatibilityMode.Off, "Disabled"),
            new(VersionCompatibilityMode.Loose, "Loose"),
            new(VersionCompatibilityMode.Strict, "Strict")
        };

        _settings.Settings.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(_settings.Settings.DarkTheme))
            {
                return;
            }

            var app = (App)Application.Current;
            app.UpdateTheme();
        };

        SaveAsync = new AsyncRelayCommand<ICloseable>(DoSaveAsync);
        ViewLogs = new RelayCommand(DoViewLogs);
    }

    public RelayCommand ViewLogs { get; }
    public AsyncRelayCommand<ICloseable> SaveAsync { get; }
    public AppSettings Settings => _settings.Settings;

    public List<ComboBoxEntry<AnalyzeMode>> AnalyzerModes { get; }

    public ComboBoxEntry<AnalyzeMode> SelectedAnalyzeMode
    {
        get => AnalyzerModes.Single(a => a.Value == _settings.Settings.AnalyzeMode);
        set => _settings.Settings.AnalyzeMode = value.Value;
    }

    public List<ComboBoxEntry<VersionCompatibilityMode>> SystemVersionModes { get; }

    public ComboBoxEntry<VersionCompatibilityMode> SelectedSystemVersionMode
    {
        get => SystemVersionModes.Single(a => a.Value == _settings.Settings.SystemVersionMode);
        set => _settings.Settings.SystemVersionMode = value.Value;
    }

    private void DoViewLogs()
    {
        _logging.OpenLoggingWindow();
    }

    private async Task DoSaveAsync(ICloseable? window)
    {
        await _settings.SaveAsync();
        window?.Close();
    }
}