using LegalAssistantApp.Data;
using LegalAssistantApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LegalAssistantApp.Services;

public class CounterpartyService
{
    private readonly AppDbContext _context;

    // Конструктор с контекстом
    public CounterpartyService(AppDbContext context)
    {
        _context = context;
    }

    // Конструктор без параметров (для DI)
    public CounterpartyService()
    {
        _context = new AppDbContext();
    }

    public async Task<List<Counterparty>> GetCounterpartiesAsync(string? search = null)
    {
        IQueryable<Counterparty> query = _context.Counterparties;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                EF.Functions.Like(c.Name, $"%{search}%") ||
                EF.Functions.Like(c.INN, $"%{search}%") ||
                EF.Functions.Like(c.OGRN, $"%{search}%"));
        }

        return await query
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Counterparty>> GetAllCounterpartiesAsync()
    {
        return await GetCounterpartiesAsync();
    }

    public async Task<List<Counterparty>> SearchCounterpartiesAsync(string search)
    {
        return await GetCounterpartiesAsync(search);
    }

    public async Task SaveCounterpartyAsync(Counterparty counterparty)
    {
        if (counterparty.Id == 0)
        {
            counterparty.CreatedDate = DateTime.UtcNow;
            counterparty.UpdatedDate = DateTime.UtcNow;
            _context.Counterparties.Add(counterparty);
        }
        else
        {
            counterparty.UpdatedDate = DateTime.UtcNow;
            _context.Counterparties.Update(counterparty);
        }

        await _context.SaveChangesAsync();
    }

    public async Task CreateCounterpartyAsync(Counterparty counterparty)
    {
        await SaveCounterpartyAsync(counterparty);
    }

    public async Task UpdateCounterpartyAsync(Counterparty counterparty)
    {
        await SaveCounterpartyAsync(counterparty);
    }

    public async Task DeleteCounterpartyAsync(int id)
    {
        var entity = await _context.Counterparties.FirstOrDefaultAsync(c => c.Id == id);
        if (entity != null)
        {
            _context.Counterparties.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<AuditHistory?> CheckWithFnsAsync(int counterpartyId, string inn, string apiKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inn) || string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var requestUri = $"https://api-fns.ru/api/egr?req={inn}&key={apiKey}";

        using var http = new HttpClient();
        var response = await http.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var j = JObject.Parse(json);
        var items = j["items"] as JArray;
        var first = items != null && items.Count > 0 ? items[0] : null;

        var shortName = first?["ЮЛ"]?["НаимСокрЮЛ"]?.ToString() ?? string.Empty;
        var ogrn = first?["ЮЛ"]?["ОГРН"]?.ToString() ?? string.Empty;
        var address = first?["ЮЛ"]?["АдресПолн"]?.ToString() ?? string.Empty;

        var summary = new
        {
            Name = shortName,
            Ogrn = ogrn,
            Address = address
        };

        var reportData = JObject.FromObject(summary).ToString();

        var history = new AuditHistory
        {
            CounterpartyId = counterpartyId,
            Source = "ФНС (через api-fns.ru)",
            RawData = json,
            ReportData = reportData,
            RiskLevel = "Unknown",
            RiskScore = 0,
            HasProblems = false,
            CheckDate = DateTime.UtcNow
        };

        _context.AuditHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);

        return history;
    }

    public async Task<AuditHistory?> GetLastAuditHistoryAsync(int counterpartyId)
    {
        return await _context.AuditHistories
            .Where(a => a.CounterpartyId == counterpartyId)
            .OrderByDescending(a => a.CheckDate)
            .FirstOrDefaultAsync();
    }
}