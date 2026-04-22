using System.Text.Json;
using FlightTracker.Models;

namespace FlightTracker.Services;

public interface IUserPreferencesService
{
    UserPreferences Load();
    void Save(UserPreferences preferences);
}

public sealed class JsonUserPreferencesService : IUserPreferencesService
{
    private readonly string _preferencesPath;

    public JsonUserPreferencesService(string? preferencesPath = null)
    {
        _preferencesPath = preferencesPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FlightTracker",
            "user-preferences.json");
    }

    public UserPreferences Load()
    {
        if (!File.Exists(_preferencesPath))
        {
            return new UserPreferences();
        }

        var json = File.ReadAllText(_preferencesPath);
        return JsonSerializer.Deserialize<UserPreferences>(json) ?? new UserPreferences();
    }

    public void Save(UserPreferences preferences)
    {
        var directory = Path.GetDirectoryName(_preferencesPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(preferences, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_preferencesPath, json);
    }
}