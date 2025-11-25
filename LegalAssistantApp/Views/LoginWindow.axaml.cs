using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LegalAssistantApp.Data;
using LegalAssistantApp.Services;
using LegalAssistantApp.ViewModels;

namespace LegalAssistantApp.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new AuthViewModel(new AuthService(new AppDbContext()));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}