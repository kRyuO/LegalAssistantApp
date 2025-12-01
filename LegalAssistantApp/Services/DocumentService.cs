using LegalAssistantApp.Data;
using LegalAssistantApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegalAssistantApp.Services;

public class DocumentService
{
    public async Task<List<Document>> GetDocumentsAsync(string? search = null)
    {
        await using var context = new AppDbContext();

        IQueryable<Document> query = context.Documents;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                EF.Functions.Like(d.Title, $"%{search}%") ||
                EF.Functions.Like(d.DocumentNumber, $"%{search}%") ||
                EF.Functions.Like(d.Tags, $"%{search}%"));
        }

        return await query
            .OrderByDescending(d => d.DocumentDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SaveDocumentAsync(Document document)
    {
        await using var context = new AppDbContext();

        if (document.Id == 0)
        {
            document.CreatedDate = DateTime.UtcNow;
            document.UpdatedDate = DateTime.UtcNow;
            context.Documents.Add(document);
        }
        else
        {
            document.UpdatedDate = DateTime.UtcNow;
            context.Documents.Update(document);
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteDocumentAsync(int id)
    {
        await using var context = new AppDbContext();
        var entity = await context.Documents.FirstOrDefaultAsync(d => d.Id == id);
        if (entity != null)
        {
            context.Documents.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
    public async Task<List<DocumentEvent>> GetUpcomingEventsAsync(DateTime? until = null)
    {
        await using var context = new AppDbContext();
        var boundary = until ?? DateTime.UtcNow.AddDays(30);

        return await context.DocumentEvents
            .Include(e => e.Document)
            .Where(e => e.Status == "Pending" && e.DueDate <= boundary)
            .OrderBy(e => e.DueDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task MarkEventCompletedAsync(int eventId)
    {
        await using var context = new AppDbContext();
        var entity = await context.DocumentEvents.FirstOrDefaultAsync(e => e.Id == eventId);
        if (entity != null)
        {
            entity.Status = "Completed";
            entity.CompletedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}
