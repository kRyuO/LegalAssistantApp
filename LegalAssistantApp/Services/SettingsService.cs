using System;
using System.IO;
using System.Text.Json;

namespace LegalAssistantApp.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "LegalAssistant");
        if (!Directory.Exists(appFolder))
            Directory.CreateDirectory(appFolder);

        _settingsPath = Path.Combine(appFolder, "settings.json");
        _settings = LoadSettings();
    }

    public AppSettings Settings => _settings;

    public void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
        }
    }

    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
        }
        return new AppSettings();
    }

    public void UpdateTheme(string theme)
    {
        _settings.Theme = theme;
        SaveSettings();
    }

    public void UpdateLanguage(string language)
    {
        _settings.Language = language;
        SaveSettings();
    }
}

public class AppSettings
{
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "ru-RU";
    public bool ShowWelcomeScreen { get; set; } = true;

    // Настройки SMTP / Email
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool SmtpUseSsl { get; set; } = true;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
}





