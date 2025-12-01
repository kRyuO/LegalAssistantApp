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
    public async Task<List<Counterparty>> GetCounterpartiesAsync(string? search = null)
    {
        await using var context = new AppDbContext();

        IQueryable<Counterparty> query = context.Counterparties;

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

    public async Task SaveCounterpartyAsync(Counterparty counterparty)
    {
        await using var context = new AppDbContext();

        if (counterparty.Id == 0)
        {
            counterparty.CreatedDate = DateTime.UtcNow;
            counterparty.UpdatedDate = DateTime.UtcNow;
            context.Counterparties.Add(counterparty);
        }
        else
        {
            counterparty.UpdatedDate = DateTime.UtcNow;
            context.Counterparties.Update(counterparty);
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteCounterpartyAsync(int id)
    {
        await using var context = new AppDbContext();
        var entity = await context.Counterparties.FirstOrDefaultAsync(c => c.Id == id);
        if (entity != null)
        {
            context.Counterparties.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Проверка контрагента по ИНН через внешний API (например, api-fns.ru или другой провайдер данных ФНС).
    /// </summary>
    /// <param name="counterpartyId">ID контрагента в локной базе</param>
    /// <param name="inn">ИНН контрагента</param>
    /// <param name="apiKey">Ключ доступа к API провайдера ФНС</param>
    public async Task<AuditHistory?> CheckWithFnsAsync(int counterpartyId, string inn, string apiKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inn) || string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        // Пример для сервиса api-fns.ru: https://api-fns.ru/api/egr?req={inn}&key={apiKey}
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

        await using var context = new AppDbContext();
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

        context.AuditHistories.Add(history);
        await context.SaveChangesAsync(cancellationToken);

        return history;
    }

    public async Task<AuditHistory?> GetLastAuditHistoryAsync(int counterpartyId)
    {
        await using var context = new AppDbContext();
        return await context.AuditHistories
            .Where(a => a.CounterpartyId == counterpartyId)
            .OrderByDescending(a => a.CheckDate)
            .FirstOrDefaultAsync();
    }
}
