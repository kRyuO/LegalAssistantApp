using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegalAssistantApp.Models;

public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string Content { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "RUB";
    public DateTime DocumentDate { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Tags { get; set; } = string.Empty;
    public bool IsConfidential { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    public int? CounterpartyId { get; set; }
    public int? DocumentTemplateId { get; set; }

    public Counterparty? Counterparty { get; set; }
    public DocumentTemplate? DocumentTemplate { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public List<DocumentEvent> DocumentEvents { get; set; } = new();
}
