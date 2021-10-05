using System.Windows;
using System.Windows.Controls;
using RefScout.Analyzer;
using RefScout.Wpf.ViewModels;

namespace RefScout.Wpf.Views;

public partial class TechnologiesTab : UserControl
{
    public static readonly DependencyProperty AnalyzerResultProperty =
        DependencyProperty.Register("AnalyzerResult",
            typeof(IAnalyzerResult),
            typeof(TechnologiesTab),
            new FrameworkPropertyMetadata(null, OnAnalyzerResultChanged));

    public TechnologiesTab()
    {
        InitializeComponent();
    }

    public IAnalyzerResult AnalyzerResult
    {
        get => (IAnalyzerResult)GetValue(AnalyzerResultProperty);
        set => SetValue(AnalyzerResultProperty, value);
    }

    private static void OnAnalyzerResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var container = (TechnologiesTab)d;
        var context = (TechnologiesTabViewModel)container.DataContext;
        context.OnNewAnalyzerResult(e.NewValue as IAnalyzerResult);
    }
}