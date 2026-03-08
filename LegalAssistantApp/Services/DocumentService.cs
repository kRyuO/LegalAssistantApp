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
    private readonly AppDbContext _context;

    // Конструктор с контекстом
    public DocumentService(AppDbContext context)
    {
        _context = context;
    }

    // Конструктор без параметров (для DI)
    public DocumentService()
    {
        _context = new AppDbContext();
    }

    public async Task<List<Document>> GetDocumentsAsync(string? search = null)
    {
        IQueryable<Document> query = _context.Documents;

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d =>
                EF.Functions.Like(d.Title, $"%{search}%") ||
                EF.Functions.Like(d.DocumentNumber, $"%{search}%") ||
                EF.Functions.Like(d.Tags, $"%{search}%"));
        }

        return await query
            .OrderByDescending(d => d.DocumentDate ?? DateTime.MinValue)
            .ThenByDescending(d => d.CreatedDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Document>> GetAllDocumentsAsync()
    {
        return await GetDocumentsAsync();
    }

    public async Task<List<Document>> SearchDocumentsAsync(string search)
    {
        return await GetDocumentsAsync(search);
    }

    public async Task SaveDocumentAsync(Document document)
    {
        if (document.Id == 0)
        {
            document.CreatedDate = DateTime.UtcNow;
            document.UpdatedDate = DateTime.UtcNow;
            _context.Documents.Add(document);
        }
        else
        {
            document.UpdatedDate = DateTime.UtcNow;
            _context.Documents.Update(document);
        }

        await _context.SaveChangesAsync();
    }

    public async Task CreateDocumentAsync(Document document)
    {
        await SaveDocumentAsync(document);
    }

    public async Task UpdateDocumentAsync(Document document)
    {
        await SaveDocumentAsync(document);
    }

    public async Task DeleteDocumentAsync(int id)
    {
        var entity = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
        if (entity != null)
        {
            _context.Documents.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<DocumentEvent>> GetUpcomingEventsAsync(DateTime? until = null)
    {
        var boundary = until ?? DateTime.UtcNow.AddDays(30);

        return await _context.DocumentEvents
            .Include(e => e.Document)
            .Where(e => e.Status == "Pending" && e.DueDate <= boundary)
            .OrderBy(e => e.DueDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task MarkEventCompletedAsync(int eventId)
    {
        var entity = await _context.DocumentEvents.FirstOrDefaultAsync(e => e.Id == eventId);
        if (entity != null)
        {
            entity.Status = "Completed";
            entity.CompletedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}