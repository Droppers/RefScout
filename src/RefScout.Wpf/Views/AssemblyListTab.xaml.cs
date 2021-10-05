using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using RefScout.Analyzer;
using RefScout.Wpf.ViewModels;

namespace RefScout.Wpf.Views;

public partial class AssemblyListTab : UserControl
{
    public static readonly DependencyProperty AnalyzerResultProperty =
        DependencyProperty.Register("AnalyzerResult",
            typeof(IAnalyzerResult),
            typeof(AssemblyListTab),
            new FrameworkPropertyMetadata(null, OnAnalyzerResultChanged));

    public AssemblyListTab()
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
        var container = (AssemblyListTab)d;
        var context = (AssemblyListTabViewModel)container.DataContext;
        context.OnNewAnalyzerResult(e.NewValue as IAnalyzerResult);
    }

    private void OnTargetUpdated(object sender, DataTransferEventArgs e) { }
}