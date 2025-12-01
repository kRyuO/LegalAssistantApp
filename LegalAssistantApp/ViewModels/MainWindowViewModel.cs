using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegalAssistantApp.Services;
using LegalAssistantApp.Views;
using System;

namespace LegalAssistantApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly CounterpartyViewModel _counterpartyViewModel;
    private readonly DocumentViewModel _documentViewModel;
    private readonly EventsViewModel _eventsViewModel;

    public MainWindowViewModel(string? currentUserName = null)
    {
        _counterpartyViewModel = new CounterpartyViewModel(new CounterpartyService());
        _documentViewModel = new DocumentViewModel(new DocumentService());
        _eventsViewModel = new EventsViewModel(new DocumentService());

        CurrentUserName = string.IsNullOrWhiteSpace(currentUserName)
            ? "Пользователь"
            : currentUserName;

        CurrentContent = this;
    }

    [ObservableProperty]
    private object _currentContent = null!;

    [ObservableProperty]
    private string _welcomeMessage = "Добро пожаловать в систему, Пользователь";

    [ObservableProperty]
    private string _currentUserName = "Пользователь";

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
    private async void ShowCounterparties()
    {
        CurrentContent = _counterpartyViewModel;
        await _counterpartyViewModel.LoadCounterpartiesAsync();
    }

    [RelayCommand]
    private async void ShowDocuments()
    {
        CurrentContent = _documentViewModel;
        await _documentViewModel.LoadDocumentsAsync();
    }

    [RelayCommand]
    private async void ShowEvents()
    {
        CurrentContent = _eventsViewModel;
        await _eventsViewModel.LoadEventsAsync();
    }

    [RelayCommand]
    private void ShowHome()
    {
        CurrentContent = this;
        WelcomeMessage = "Добро пожаловать в систему, Пользователь";
    }
}