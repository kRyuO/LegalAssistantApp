using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Services;
using LegalAssistantApp.Views;
using System.Threading.Tasks;

namespace LegalAssistantApp.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _errorMessage = "";

    public AuthViewModel()
    {
        _authService = new AuthService();
    }

    public AuthViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите логин и пароль";
            return;
        }

        var user = await _authService.AuthenticateAsync(Username, Password);
        if (user != null)
        {
            ErrorMessage = "";

            var mainWindow = CreateMainWindow(user);
            mainWindow.Show();

            CloseLoginWindow();
        }
        else
        {
            ErrorMessage = "Неверный логин или пароль";
        }
    }

    [RelayCommand]
    private void UseTestCredentials()
    {
        Username = "admin";
        Password = "admin123";
        ErrorMessage = "Тестовые данные заполнены. Нажмите 'Войти'";
    }

    private MainWindow CreateMainWindow(Models.User user)
    {
        var context = new Data.AppDbContext();
        var counterpartyService = new CounterpartyService(context);
        var documentService = new DocumentService(context);
        var eventService = new EventService(context);

        var mainWindowViewModel = new MainWindowViewModel(
            counterpartyService,
            documentService,
            eventService,
            user.FullName,
            user.Id,
            user.Role?.Name ?? string.Empty
        );

        return new MainWindow
        {
            DataContext = mainWindowViewModel
        };
    }

    private void CloseLoginWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}