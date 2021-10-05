using System.Threading.Tasks;

namespace RefScout.Wpf.Services;

public interface ISettingsService
{
    AppSettings Settings { get; set; }
    Task SaveAsync();
    Task LoadAsync();
}