using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Services;
using LegalAssistantApp.Localization;
using LegalAssistantApp.Themes;
using System;
using System.Collections.Generic;

namespace LegalAssistantApp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _selectedLanguage = _settingsService.Settings.Language;
        _selectedTheme = _settingsService.Settings.Theme;
        
        // Инициализируем язык при создании
        ApplyLanguage(_selectedLanguage);
        
        // Подписываемся на изменения языка
        LocalizationService.Instance.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(LocalizationService.CurrentLanguage))
            {
                OnPropertyChanged(nameof(LanguageLabel));
                OnPropertyChanged(nameof(SaveSettingsButton));
                OnPropertyChanged(nameof(ApplicationSettingsTitle));
                OnPropertyChanged(nameof(ThemeLabel));
            }
        };
    }

    [ObservableProperty]
    private string _selectedLanguage = "ru-RU";

    [ObservableProperty]
    private string _selectedTheme = "Light";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public List<string> Languages { get; } = new List<string>
    {
        "ru-RU",
        "en-US"
    };

    public List<string> Themes { get; } = new List<string>
    {
        "Light",
        "Dark"
    };

    public string LanguageLabel => LocalizationService.Instance["Language"];
    public string SaveSettingsButton => LocalizationService.Instance["SaveSettings"];
    public string ApplicationSettingsTitle => LocalizationService.Instance["ApplicationSettings"];
    public string ThemeLabel => LocalizationService.Instance["Theme"];

    // Email / SMTP настройки
    public string SmtpHost
    {
        get => _settingsService.Settings.SmtpHost;
        set
        {
            if (_settingsService.Settings.SmtpHost != value)
            {
                _settingsService.Settings.SmtpHost = value;
                OnPropertyChanged();
            }
        }
    }

    public int SmtpPort
    {
        get => _settingsService.Settings.SmtpPort;
        set
        {
            if (_settingsService.Settings.SmtpPort != value)
            {
                _settingsService.Settings.SmtpPort = value;
                OnPropertyChanged();
            }
        }
    }

    public bool SmtpUseSsl
    {
        get => _settingsService.Settings.SmtpUseSsl;
        set
        {
            if (_settingsService.Settings.SmtpUseSsl != value)
            {
                _settingsService.Settings.SmtpUseSsl = value;
                OnPropertyChanged();
            }
        }
    }

    public string SmtpUser
    {
        get => _settingsService.Settings.SmtpUser;
        set
        {
            if (_settingsService.Settings.SmtpUser != value)
            {
                _settingsService.Settings.SmtpUser = value;
                OnPropertyChanged();
            }
        }
    }

    public string SmtpPassword
    {
        get => _settingsService.Settings.SmtpPassword;
        set
        {
            if (_settingsService.Settings.SmtpPassword != value)
            {
                _settingsService.Settings.SmtpPassword = value;
                OnPropertyChanged();
            }
        }
    }

    public string FromEmail
    {
        get => _settingsService.Settings.FromEmail;
        set
        {
            if (_settingsService.Settings.FromEmail != value)
            {
                _settingsService.Settings.FromEmail = value;
                OnPropertyChanged();
            }
        }
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        if (_settingsService != null && !string.IsNullOrEmpty(value))
        {
            _settingsService.UpdateLanguage(value);
            ApplyLanguage(value);
            
            var loc = LocalizationService.Instance;
            StatusMessage = loc["LanguageChanged"];
            
            // Уведомляем все ViewModel о смене языка
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (_settingsService != null && !string.IsNullOrEmpty(value))
        {
            _settingsService.UpdateTheme(value);
            ThemeManager.Instance.ApplyTheme(value);

            var loc = LocalizationService.Instance;
            StatusMessage = loc["ThemeChanged"];
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    private void ApplyLanguage(string languageCode)
    {
        var loc = LocalizationService.Instance;
        loc.CurrentLanguage = languageCode;
        
        // Уведомляем все окна о смене языка
        if (Application.Current != null && 
            Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                // Обновляем DataContext для обновления всех биндингов
                if (window.DataContext is MainWindowViewModel mainVM)
                {
                    mainVM.OnLanguageChanged();
                }
            }
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _settingsService.SaveSettings();
        var loc = LocalizationService.Instance;
        StatusMessage = loc["SettingsSaved"];
    }
}



