using System.Linq;
using System.Windows;
using RefScout.Wpf.ViewModels;

namespace RefScout.Wpf.Views;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnDragEnter(object sender, DragEventArgs e)
    {
        var context = (MainWindowViewModel)DataContext;
        context.IsDragging = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        var context = (MainWindowViewModel)DataContext;
        context.IsDragging = false;
    }

    private void HandleFileDrop(object sender, DragEventArgs e)
    {
        var context = (MainWindowViewModel)DataContext;
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            context.IsDragging = false;
            return;
        }

        var files = e.Data.GetData(DataFormats.FileDrop) as string[];
        var file = files?.FirstOrDefault();
        if (file == null)
        {
            context.IsDragging = false;
            return;
        }

        context.IsDragging = false;
        if (context.AnalyzeAssembly.CanExecute(file))
        {
            context.AnalyzeAssembly.Execute(file);
        }
    }
}