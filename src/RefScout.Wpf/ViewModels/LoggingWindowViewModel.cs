using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using RefScout.Core.Logging;
using RefScout.Wpf.Services;

namespace RefScout.Wpf.ViewModels;

internal class LoggingWindowViewModel : ObservableObject
{
    private readonly ILoggingService _logging;

    public LoggingWindowViewModel(ILoggingService logging)
    {
        _logging = logging;
        Save = new AsyncRelayCommand(DoSaveAsync);
    }

    public AsyncRelayCommand Save { get; }
    public ObservableCollection<LogEntry> Entries => _logging.Entries;

    private async Task DoSaveAsync()
    {
        await _logging.SaveAsync();
    }
}