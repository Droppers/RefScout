using System.Windows;
using System.Windows.Controls;

namespace RefScout.Wpf.Views.Controls;

public partial class Icon : UserControl
{
    public static readonly DependencyProperty IconNameProperty =
        DependencyProperty.Register("IconName",
            typeof(string),
            typeof(Icon),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty FitProperty =
        DependencyProperty.Register("Fit",
            typeof(bool),
            typeof(Icon),
            new FrameworkPropertyMetadata(false));

    public Icon()
    {
        InitializeComponent();
    }

    public string IconName
    {
        get => (string)GetValue(IconNameProperty);
        set => SetValue(IconNameProperty, value);
    }

    public bool Fit
    {
        get => (bool)GetValue(FitProperty);
        set => SetValue(FitProperty, value);
    }
}