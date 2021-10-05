using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using RefScout.Core;
using RefScout.Core.Logging;

namespace RefScout.Wpf.Services;

public class SettingsService : ISettingsService
{
    private const string ConfigFileName = "settings.json";

    private static readonly string PathToSettings = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.ApplicationName,
        ConfigFileName);

    public SettingsService()
    {
        Settings = new AppSettings();
    }

    public AppSettings Settings { get; set; }

    public async Task SaveAsync()
    {
        try
        {
            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, Settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            Directory.CreateDirectory(Path.GetDirectoryName(PathToSettings)!);
            await File.WriteAllTextAsync(PathToSettings, json);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Could not not save settings.json");
        }
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(PathToSettings))
        {
            return;
        }

        try
        {
            await using var stream = File.OpenRead(PathToSettings);
            var obj = await JsonSerializer.DeserializeAsync<AppSettings>(stream);
            if (obj != null)
            {
                Settings = obj;
            }
            else
            {
                Logger.Error("Could not parse settings.json.");
            }
        }
        catch (FileNotFoundException) { }
        catch (Exception e)
        {
            Logger.Error(e, "Could not not load settings.json");
        }
    }
}