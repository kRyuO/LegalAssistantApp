using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegalAssistantApp.Models;

public class AuditHistory
{
    public int Id { get; set; }
    public DateTime CheckDate { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;
    public string RawData { get; set; } = string.Empty;
    public string ReportData { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Unknown";
    public int RiskScore { get; set; }
    public string Findings { get; set; } = string.Empty;
    public bool HasProblems { get; set; } = false;

    public int CounterpartyId { get; set; }
    public Counterparty Counterparty { get; set; } = null!;
}
