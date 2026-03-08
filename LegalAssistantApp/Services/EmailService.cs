using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using LegalAssistantApp.Data;
using LegalAssistantApp.Models;

namespace LegalAssistantApp.Services;

public class EmailService
{
    private readonly SettingsService _settingsService;

    public EmailService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task SendReminderEmailAsync(DocumentEvent documentEvent)
    {
        if (string.IsNullOrWhiteSpace(documentEvent.NotificationEmail))
        {
            // Нечего отправлять — нет адреса получателя
            return;
        }

        var settings = _settingsService.Settings;
        if (string.IsNullOrWhiteSpace(settings.SmtpHost) ||
            string.IsNullOrWhiteSpace(settings.SmtpUser) ||
            string.IsNullOrWhiteSpace(settings.SmtpPassword))
        {
            // SMTP не настроен — пропускаем отправку, но не помечаем как отправленное
            await LogEmailAsync(documentEvent, "Skipped", "SMTP settings are not configured");
            return;
        }

        var from = string.IsNullOrWhiteSpace(settings.FromEmail)
            ? settings.SmtpUser
            : settings.FromEmail;

        var subject = $"Напоминание: {documentEvent.Title}";
        var body =
            $"Напоминание по событию \"{documentEvent.Title}\" на {documentEvent.EventDate:dd.MM.yyyy HH:mm}.\n\n" +
            $"{documentEvent.Description}";

        using var message = new MailMessage(from, documentEvent.NotificationEmail)
        {
            Subject = subject,
            Body = body
        };

        // Попробуем прикрепить файл документа, если он есть
        if (documentEvent.Document != null &&
            !string.IsNullOrWhiteSpace(documentEvent.Document.FilePath))
        {
            try
            {
                var attachment = new Attachment(documentEvent.Document.FilePath);
                message.Attachments.Add(attachment);
            }
            catch
            {
                // Не валим отправку, если файл недоступен
            }
        }

        string status;
        string? error = null;

        try
        {
            using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                EnableSsl = settings.SmtpUseSsl,
                Credentials = new NetworkCredential(settings.SmtpUser, settings.SmtpPassword)
            };

            await client.SendMailAsync(message);
            status = "Sent";
        }
        catch (Exception ex)
        {
            status = "Failed";
            error = ex.Message;
        }

        await LogEmailAsync(documentEvent, status, error);
    }

    private static async Task LogEmailAsync(DocumentEvent documentEvent, string status, string? errorMessage)
    {
        using var context = new AppDbContext();

        var log = new EmailLog
        {
            To = documentEvent.NotificationEmail,
            Subject = $"Напоминание: {documentEvent.Title}",
            Body = documentEvent.Description,
            AttachmentPath = documentEvent.Document?.FilePath,
            DocumentId = documentEvent.DocumentId,
            DocumentEventId = documentEvent.Id,
            SentAt = DateTime.UtcNow,
            Status = status,
            ErrorMessage = errorMessage
        };

        context.EmailLogs.Add(log);

        var entity = await context.DocumentEvents.FindAsync(documentEvent.Id);
        if (entity != null)
        {
            if (status == "Sent")
            {
                entity.ReminderEmailSent = true;
                entity.ReminderLastSentDate = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
    }
}

