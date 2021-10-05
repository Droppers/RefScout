using System.ComponentModel;
using System.Runtime.CompilerServices;
using RefScout.Analyzer;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Visualizers;

namespace RefScout.Wpf;

public class AppSettings : INotifyPropertyChanged
{
    private bool _darkTheme = true;
    private AnalyzeMode _analyzeMode;
    private VersionCompatibilityMode _systemVersionMode;
    private bool _showReferencedBy;

    private VisualizeMode _visualizeMode = VisualizeMode.All;
    private bool _graphShowDotNetFramework;
    private bool _graphTopBottom = true;

    public bool DarkTheme
    {
        get => _darkTheme;
        set
        {
            _darkTheme = value;
            OnPropertyChanged();
        }
    }

    public AnalyzeMode AnalyzeMode
    {
        get => _analyzeMode;
        set
        {
            _analyzeMode = value;
            OnPropertyChanged();
        }
    }

    public VersionCompatibilityMode SystemVersionMode
    {
        get => _systemVersionMode;
        set
        {
            _systemVersionMode = value;
            OnPropertyChanged();
        }
    }

    public bool ShowReferencedBy
    {
        get => _showReferencedBy;
        set
        {
            _showReferencedBy = value;
            OnPropertyChanged();
        }
    }

    public VisualizeMode VisualizeMode
    {
        get => _visualizeMode;
        set
        {
            _visualizeMode = value;
            OnPropertyChanged();
        }
    }

    public bool GraphTopBottom
    {
        get => _graphTopBottom;
        set
        {
            _graphTopBottom = value;
            OnPropertyChanged();
        }
    }

    public bool GraphShowTargetFramework
    {
        get => _graphShowDotNetFramework;
        set
        {
            _graphShowDotNetFramework = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}