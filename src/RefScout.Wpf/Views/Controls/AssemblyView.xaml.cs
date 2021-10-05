using System.Windows;
using System.Windows.Controls;
using RefScout.Analyzer;

namespace RefScout.Wpf.Views.Controls;

public partial class AssemblyView : UserControl
{
    public static readonly DependencyProperty InputAssemblyProperty =
        DependencyProperty.Register("InputAssembly",
            typeof(Assembly),
            typeof(AssemblyView));

    public static readonly DependencyProperty ReferencedByProperty =
        DependencyProperty.Register("ReferencedBy",
            typeof(bool),
            typeof(AssemblyView));

    public AssemblyView()
    {
        InitializeComponent();
    }

    public Assembly InputAssembly
    {
        get => (Assembly)GetValue(InputAssemblyProperty);
        set => SetValue(InputAssemblyProperty, value);
    }

    public bool ReferencedBy
    {
        get => (bool)GetValue(ReferencedByProperty);
        set => SetValue(ReferencedByProperty, value);
    }
}