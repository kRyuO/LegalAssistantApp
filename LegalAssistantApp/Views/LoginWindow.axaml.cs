using Avalonia.Controls;
using LegalAssistantApp.ViewModels;

namespace LegalAssistantApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Создаем AuthViewModel без AuthService (используем конструктор без параметров)
            DataContext = new AuthViewModel();
        }
    }
}