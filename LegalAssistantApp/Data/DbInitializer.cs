using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegalAssistantApp.Models;
using LegalAssistantApp.Services;
using Microsoft.EntityFrameworkCore;

namespace LegalAssistantApp.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Users.AnyAsync())
            return;

        var authService = new AuthService(context);

        var roles = new List<Role>
        {
            new() { Id = 1, Name = "Admin", Description = "Администратор системы", IsSystemRole = true },
            new() { Id = 2, Name = "Manager", Description = "Менеджер", IsSystemRole = false },
            new() { Id = 3, Name = "Lawyer", Description = "Юрист", IsSystemRole = false },
            new() { Id = 4, Name = "Viewer", Description = "Только просмотр", IsSystemRole = false },
        };
        context.Roles.AddRange(roles);

        var users = new List<User>();

        void AddUser(int id, string username, string fullName, string email, int roleId, string password)
        {
            var (hash, salt) = authService.HashPassword(password);
            users.Add(new User
            {
                Id = id,
                Username = username,
                FullName = fullName,
                FirstName = fullName.Split(' ').FirstOrDefault() ?? fullName,
                LastName = fullName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                Email = email,
                RoleId = roleId,
                PasswordHash = hash,
                Salt = salt,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });
        }

        AddUser(1, "admin", "Администратор Системы", "admin@example.com", 1, "admin123");
        AddUser(2, "manager1", "Менеджер Иванов", "manager1@example.com", 2, "Pass123!");
        AddUser(3, "manager2", "Менеджер Петров", "manager2@example.com", 2, "Pass123!");
        AddUser(4, "lawyer1", "Юрист Сидоров", "lawyer1@example.com", 3, "Pass123!");
        AddUser(5, "lawyer2", "Юрист Кузнецов", "lawyer2@example.com", 3, "Pass123!");
        AddUser(6, "viewer1", "Наблюдатель Один", "viewer1@example.com", 4, "Pass123!");
        AddUser(7, "viewer2", "Наблюдатель Два", "viewer2@example.com", 4, "Pass123!");
        AddUser(8, "user1", "Пользователь Один", "user1@example.com", 3, "Pass123!");
        AddUser(9, "user2", "Пользователь Два", "user2@example.com", 2, "Pass123!");
        AddUser(10, "user3", "Пользователь Три", "user3@example.com", 4, "Pass123!");

        context.Users.AddRange(users);

        var counterparties = new List<Counterparty>();
        for (int i = 1; i <= 10; i++)
        {
            counterparties.Add(new Counterparty
            {
                Id = i,
                Name = $"Контрагент {i}",
                FullName = $"ООО \"Контрагент {i}\"",
                INN = $"{7700000000 + i:D10}",
                KPP = $"{770001000 + i:D9}",
                OGRN = $"{1027700000000 + i:D13}",
                LegalAddress = $"г. Москва, ул. Примерная, д.{i}",
                ActualAddress = $"г. Москва, ул. Рабочая, д.{i}",
                Phone = $"+7 (495) 000-0{i:D2}",
                Email = $"cp{i}@example.com",
                DirectorName = $"Директор {i}",
                Status = "Active",
                RiskLevel = i % 3 == 0 ? "High" : i % 2 == 0 ? "Medium" : "Low",
                Notes = "Тестовый контрагент для демонстрации.",
                CreatedByUserId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-i),
                UpdatedDate = DateTime.UtcNow.AddDays(-i + 1),
                IsActive = true
            });
        }
        context.Counterparties.AddRange(counterparties);

        var documents = new List<Document>();
        for (int i = 1; i <= 10; i++)
        {
            documents.Add(new Document
            {
                Id = i,
                Title = $"Договор №{i}",
                DocumentNumber = $"CN-{2024 + i}-{i:D3}",
                DocumentType = i % 2 == 0 ? "Договор" : "Акт",
                Status = i % 3 == 0 ? "На согласовании" : "Активен",
                DocumentDate = DateTime.Today.AddDays(-i),
                Amount = 10_000 + i * 1000,
                Currency = "RUB",
                Tags = "тест,демо",
                IsConfidential = i % 2 == 0,
                Content = "Тестовое текстовое описание документа для демонстрации.",
                CounterpartyId = i,
                CreatedByUserId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-i),
                UpdatedDate = DateTime.UtcNow.AddDays(-i + 1),
                FileName = $"document_{i}.pdf",
                FileExtension = ".pdf",
                FilePath = "",
                FileSize = null,
                FileCreatedDate = null,
                FileModifiedDate = null
            });
        }
        context.Documents.AddRange(documents);

        var events = new List<DocumentEvent>();
        for (int i = 1; i <= 10; i++)
        {
            events.Add(new DocumentEvent
            {
                Id = i,
                Title = $"Событие по документу {i}",
                Description = "Тестовое событие для демонстрации календаря.",
                EventDate = DateTime.Now.AddDays(i),
                EventType = i % 2 == 0 ? "Срок" : "Напоминание",
                Status = "Активно",
                IsCompleted = false,
                DocumentId = i,
                CounterpartyId = i,
                HasReminder = true,
                ReminderDaysBefore = 2,
                ReminderOffsetMinutes = 2 * 24 * 60, // 2 дня
                ReminderDate = DateTime.Now.AddDays(i - 2),
                NotificationEmail = "notify@example.com",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "system",
                Priority = i % 3
            });
        }
        context.DocumentEvents.AddRange(events);

        var templates = new List<DocumentTemplate>();
        for (int i = 1; i <= 10; i++)
        {
            templates.Add(new DocumentTemplate
            {
                Id = i,
                Name = $"Шаблон {i}",
                Description = "Тестовый шаблон документа.",
                Category = i % 2 == 0 ? "Договор" : "Иск",
                FilePath = $"Templates/template_{i}.docx",
                Variables = "ClientName;Date;Amount",
                Version = "1.0",
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-i),
                UpdatedDate = DateTime.UtcNow.AddDays(-i + 1),
                CreatedByUserId = 1
            });
        }
        context.DocumentTemplates.AddRange(templates);

        var histories = new List<AuditHistory>();
        for (int i = 1; i <= 10; i++)
        {
            histories.Add(new AuditHistory
            {
                Id = i,
                CounterpartyId = i,
                Source = "Тестовая проверка",
                RawData = "{}",
                ReportData = "{}",
                RiskLevel = "Low",
                RiskScore = 10,
                Findings = "Проблем не обнаружено",
                HasProblems = false,
                CheckDate = DateTime.UtcNow.AddDays(-i)
            });
        }
        context.AuditHistories.AddRange(histories);

        var logs = new List<AuditLog>();
        for (int i = 1; i <= 10; i++)
        {
            logs.Add(new AuditLog
            {
                Id = i,
                Action = "Create",
                EntityType = "Document",
                EntityId = i,
                OldValues = "",
                NewValues = "{ \"Title\": \"Test\" }",
                IPAddress = "127.0.0.1",
                UserAgent = "Seeder",
                Timestamp = DateTime.UtcNow.AddMinutes(-i),
                UserId = 1
            });
        }
        context.AuditLogs.AddRange(logs);

        await context.SaveChangesAsync();
    }
}





