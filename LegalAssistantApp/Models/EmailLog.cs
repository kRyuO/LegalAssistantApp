using System;
using System.ComponentModel.DataAnnotations;

namespace LegalAssistantApp.Models;

public class EmailLog
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string To { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AttachmentPath { get; set; }

    public int? DocumentId { get; set; }
    public Document? Document { get; set; }

    public int? DocumentEventId { get; set; }
    public DocumentEvent? DocumentEvent { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string Status { get; set; } = "Unknown"; // Sent / Failed / Skipped

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
}

