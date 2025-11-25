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

            // Переход на главное окно
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Закрытие текущего окна входа
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
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
        else
        {
            ErrorMessage = "Неверный логин или пароль";
        }
    }
}