using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LegalAssistantApp.Data;
using LegalAssistantApp.Services;
using LegalAssistantApp.ViewModels;
using LegalAssistantApp.Views;
using System;
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

        if (!context.Counterparties.Any())
        {
            var demoCounterparty = new Models.Counterparty
            {
                Name = "ООО \"Демо-контрагент\"",
                FullName = "Общество с ограниченной ответственностью \"Демо-контрагент\"",
                INN = "7700000000",
                KPP = "770001001",
                OGRN = "1027700000000",
                LegalAddress = "г. Москва, ул. Примерная, д. 1",
                ActualAddress = "г. Москва, ул. Примерная, д. 1",
                Phone = "+7 (495) 000-00-00",
                Email = "info@example.com",
                DirectorName = "Иванов И.И.",
                Status = "Active",
                RiskLevel = "Low",
                Notes = "Демонстрационный контрагент",
                CreatedByUserId = 1
            };

            context.Counterparties.Add(demoCounterparty);
            await context.SaveChangesAsync();

            var demoDocument = new Models.Document
            {
                Title = "Демо-договор поставки",
                DocumentNumber = "Д-001/2025",
                DocumentType = "Договор",
                Status = "Draft",
                Content = "Текст демо-договора...",
                DocumentDate = DateTime.UtcNow.Date,
                Tags = "демо;договор",
                CreatedByUserId = 1,
                CounterpartyId = demoCounterparty.Id
            };

            context.Documents.Add(demoDocument);

            context.DocumentEvents.Add(new Models.DocumentEvent
            {
                Document = demoDocument,
                EventType = "Срок подачи иска",
                Title = "Крайний срок подачи иска",
                Description = "Пример контрольной даты по документу.",
                DueDate = DateTime.UtcNow.Date.AddDays(10),
                Status = "Pending",
                Priority = 1
            });

            await context.SaveChangesAsync();
        }
    }
}