using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RefScout.Core.Logging;

namespace RefScout.Wpf.Services;

internal interface ILoggingService
{
    ObservableCollection<LogEntry> Entries { get; }

    Task SaveAsync();
    void OpenLoggingWindow();
}