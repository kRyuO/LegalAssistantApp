using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LegalAssistantApp.Data;
using LegalAssistantApp.ViewModels;
using LegalAssistantApp.Views;
using LegalAssistantApp.Services;
using LegalAssistantApp.Localization;
using LegalAssistantApp.Themes;

namespace LegalAssistantApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            using (var context = new AppDbContext())
            {
                await DbSchemaUpdater.EnsureUpToDateAsync(context);
                await DbInitializer.InitializeAsync(context);
            }

            var settingsService = new SettingsService();
            var settings = settingsService.Settings;

            // Инициализация темы и языка приложения
            ThemeManager.Instance.Initialize(this);
            ThemeManager.Instance.ApplyTheme(settings.Theme);
            LocalizationService.Instance.CurrentLanguage = settings.Language;

            var loginWindow = new LoginWindow
            {
                DataContext = new AuthViewModel()
            };

            desktop.MainWindow = loginWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}