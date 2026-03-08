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

        var hasUsers = await context.Users.AnyAsync();
        AuthService? authService = hasUsers ? null : new AuthService(context);

        if (!hasUsers && authService != null)
        {
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
        }

        if (!await context.Counterparties.AnyAsync())
        {
            var counterparties = new List<Counterparty>();
            for (int i = 1; i <= 10; i++)
            {
                counterparties.Add(new Counterparty
                {
                    Name = $"Контрагент {i}",
                    Type = i % 2 == 0 ? "Юр.лицо" : "Физ.лицо",
                    FullName = i % 2 == 0 ? $"ООО \"Контрагент {i}\"" : $"ИП Контрагент {i}",
                    INN = $"{7700000000 + i:D10}",
                    KPP = $"{770001000 + i:D9}",
                    OGRN = $"{1027700000000 + i:D13}",
                    LegalAddress = $"г. Москва, ул. Примерная, д.{i}",
                    ActualAddress = $"г. Москва, ул. Рабочая, д.{i}",
                    ContactPerson = $"Контакт {i}",
                    Phone = $"+7 (495) 000-0{i:D2}",
                    Email = $"cp{i}@example.com",
                    DirectorName = $"Директор {i}",
                    Status = "Активен",
                    RiskLevel = i % 3 == 0 ? "Высокий" : i % 2 == 0 ? "Средний" : "Низкий",
                    Notes = "Тестовый контрагент для демонстрации.",
                    CreatedByUserId = 1,
                    CreatedDate = DateTime.UtcNow.AddDays(-i),
                    UpdatedDate = DateTime.UtcNow.AddDays(-i + 1),
                    IsActive = true
                });
            }
            context.Counterparties.AddRange(counterparties);
        }

        if (!await context.Documents.AnyAsync())
        {
            var documents = new List<Document>();
            for (int i = 1; i <= 10; i++)
            {
                documents.Add(new Document
                {
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
        }

        // События — добавим тестовые, если событий нет вообще или нет ни одного предстоящего
        var hasAnyEvents = await context.DocumentEvents.AnyAsync();
        var hasUpcomingEvents = await context.DocumentEvents.AnyAsync(e => !e.IsCompleted && e.EventDate >= DateTime.Now);
        if (!hasAnyEvents || !hasUpcomingEvents)
        {
            var events = new List<DocumentEvent>();
            for (int i = 1; i <= 10; i++)
            {
                events.Add(new DocumentEvent
                {
                    Title = $"Событие по документу {i}",
                    Description = "Тестовое событие для демонстрации календаря.",
                    EventDate = DateTime.Now.AddDays(i).Date.AddHours(12),
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
        }

        if (!await context.DocumentTemplates.AnyAsync())
        {
            var templates = new List<DocumentTemplate>();
            for (int i = 1; i <= 10; i++)
            {
                templates.Add(new DocumentTemplate
                {
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
        }

        if (!await context.AuditHistories.AnyAsync())
        {
            var histories = new List<AuditHistory>();
            for (int i = 1; i <= 10; i++)
            {
                histories.Add(new AuditHistory
                {
                    CounterpartyId = i,
                    Source = "Тестовая проверка",
                    RawData = "{}",
                    ReportData = "{}",
                    RiskLevel = "Низкий",
                    RiskScore = 10,
                    Findings = "Проблем не обнаружено",
                    HasProblems = false,
                    CheckDate = DateTime.UtcNow.AddDays(-i)
                });
            }
            context.AuditHistories.AddRange(histories);
        }

        if (!await context.AuditLogs.AnyAsync())
        {
            var logs = new List<AuditLog>();
            for (int i = 1; i <= 10; i++)
            {
                logs.Add(new AuditLog
                {
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
        }

        await context.SaveChangesAsync();

        // Приводим существующие данные контрагентов к русским значениям (один раз)
        var allCounterparties = await context.Counterparties.ToListAsync();
        var changed = false;
        foreach (var c in allCounterparties)
        {
            if (c.Status == "Active") { c.Status = "Активен"; changed = true; }
            else if (c.Status == "Inactive") { c.Status = "Неактивен"; changed = true; }
            if (c.RiskLevel == "Low") { c.RiskLevel = "Низкий"; changed = true; }
            else if (c.RiskLevel == "Medium") { c.RiskLevel = "Средний"; changed = true; }
            else if (c.RiskLevel == "High") { c.RiskLevel = "Высокий"; changed = true; }
            else if (c.RiskLevel == "Unknown") { c.RiskLevel = "Неизвестен"; changed = true; }
        }
        if (changed)
            await context.SaveChangesAsync();
    }
}





