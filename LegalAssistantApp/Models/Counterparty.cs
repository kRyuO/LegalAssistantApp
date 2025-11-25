using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegalAssistantApp.Models;

public class Counterparty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string INN { get; set; } = string.Empty;
    public string KPP { get; set; } = string.Empty;
    public string OGRN { get; set; } = string.Empty;
    public string LegalAddress { get; set; } = string.Empty;
    public string ActualAddress { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DirectorName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string RiskLevel { get; set; } = "Unknown";
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    public List<Document> Documents { get; set; } = new();
    public List<AuditHistory> AuditHistories { get; set; } = new();
    public User CreatedByUser { get; set; } = null!;
}
