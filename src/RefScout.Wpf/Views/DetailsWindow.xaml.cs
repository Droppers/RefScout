using System.Windows;

namespace RefScout.Wpf.Views;

public partial class DetailsWindow : Window
{
    public DetailsWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}