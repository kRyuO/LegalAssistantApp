using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LegalAssistantApp.ViewModels;
using LegalAssistantApp.Models;

namespace LegalAssistantApp.Views;

public partial class MainWindow : Window
{
    public MainWindow(string? userName = null)
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(userName);
    }

    public MainWindow(User user) : this(user.Username) { }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}