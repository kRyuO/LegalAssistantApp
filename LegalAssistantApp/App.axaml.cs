using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LegalAssistantApp.Data;
using LegalAssistantApp.Services;
using LegalAssistantApp.ViewModels;
using LegalAssistantApp.Views;
using System.Linq;

namespace LegalAssistantApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        InitializeDatabase();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var loginWindow = new LoginWindow();
            loginWindow.DataContext = new AuthViewModel(new AuthService(new AppDbContext()));
            desktop.MainWindow = loginWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void InitializeDatabase()
    {
        using var context = new AppDbContext();
        var authService = new AuthService(context);

        context.Database.EnsureCreated();

        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Models.Role { Name = "Admin" },
                new Models.Role { Name = "Lawyer" },
                new Models.Role { Name = "Viewer" }
            );
            await context.SaveChangesAsync();
        }

        await authService.CreateTestUserAsync();
    }
}