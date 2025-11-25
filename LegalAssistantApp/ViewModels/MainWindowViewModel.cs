using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Views;
using System;

namespace LegalAssistantApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _welcomeMessage = "Добро пожаловать в систему, Пользователь";

    [RelayCommand]
    private void Logout()
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();

        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
    }

    [RelayCommand]
    private void ShowCounterparties()
    {
        WelcomeMessage = "Раздел 'Контрагенты' - в разработке";
    }

    [RelayCommand]
    private void ShowDocuments()
    {
        WelcomeMessage = "Раздел 'Документы' - в разработке";
    }

    [RelayCommand]
    private void ShowHome()
    {
        WelcomeMessage = "Добро пожаловать в систему, Пользователь";
    }
}