using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegalAssistantApp.Models;

public class DocumentEvent
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "Pending";
    public int Priority { get; set; }
    public string ReminderSettings { get; set; } = string.Empty;
    public bool Notified { get; set; } = false;

    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
}
