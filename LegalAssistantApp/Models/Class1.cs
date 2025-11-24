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
    public string INN { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
