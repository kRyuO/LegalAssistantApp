using LegalAssistantApp.Data;
using LegalAssistantApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegalAssistantApp.Services
{
    public class EventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }

        // Получение всех событий
        public async Task<List<DocumentEvent>> GetAllEventsAsync()
        {
            return await _context.DocumentEvents
                .Include(e => e.Document)
                .Include(e => e.Counterparty)
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        // Получение предстоящих событий
        public async Task<List<DocumentEvent>> GetUpcomingEventsAsync(DateTime? untilDate = null)
        {
            var query = _context.DocumentEvents
                .Include(e => e.Document)
                .Include(e => e.Counterparty)
                .Where(e => !e.IsCompleted && e.EventDate >= DateTime.Now);

            if (untilDate.HasValue)
            {
                query = query.Where(e => e.EventDate <= untilDate.Value);
            }

            return await query
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        // Получение событий, требующих напоминания
        public async Task<List<DocumentEvent>> GetEventsForReminderAsync()
        {
            var now = DateTime.Now;
            return await _context.DocumentEvents
                .Include(e => e.Document)
                .Include(e => e.Counterparty)
                .Where(e => !e.IsCompleted &&
                            e.HasReminder &&
                            e.ReminderDate.HasValue &&
                            e.ReminderDate <= now &&
                            !e.ReminderEmailSent)
                .ToListAsync();
        }

        // Создание события
        public async Task<DocumentEvent> CreateEventAsync(DocumentEvent documentEvent)
        {
            ApplyReminderSettings(documentEvent);

            _context.DocumentEvents.Add(documentEvent);
            await _context.SaveChangesAsync();
            return documentEvent;
        }

        // Обновление события
        public async Task UpdateEventAsync(DocumentEvent documentEvent)
        {
            ApplyReminderSettings(documentEvent);

            _context.Entry(documentEvent).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        private static void ApplyReminderSettings(DocumentEvent documentEvent)
        {
            if (!documentEvent.HasReminder)
            {
                documentEvent.ReminderDate = null;
                documentEvent.ReminderOffsetMinutes = null;
                documentEvent.ReminderDaysBefore = null;
                return;
            }

            // Приоритет — новое поле с минутами
            if (documentEvent.ReminderOffsetMinutes.HasValue)
            {
                documentEvent.ReminderDate = documentEvent.EventDate
                    .AddMinutes(-documentEvent.ReminderOffsetMinutes.Value);
            }
            else if (documentEvent.ReminderDaysBefore.HasValue)
            {
                documentEvent.ReminderDate = documentEvent.EventDate
                    .AddDays(-documentEvent.ReminderDaysBefore.Value);
            }
        }

        // Отметить событие как выполненное
        public async Task CompleteEventAsync(int eventId)
        {
            var documentEvent = await _context.DocumentEvents.FindAsync(eventId);
            if (documentEvent != null)
            {
                documentEvent.IsCompleted = true;
                documentEvent.Status = "Завершено";
                documentEvent.CompletedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // Поиск событий
        public async Task<List<DocumentEvent>> SearchEventsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllEventsAsync();

            var query = _context.DocumentEvents
                .Include(e => e.Document)
                .Include(e => e.Counterparty)
                .AsQueryable();

            var searchLower = searchText.ToLowerInvariant();
            query = query.Where(e =>
                e.Title.ToLower().Contains(searchLower) ||
                e.Description.ToLower().Contains(searchLower) ||
                (e.Document != null && e.Document.Title.ToLower().Contains(searchLower)) ||
                (e.Counterparty != null && e.Counterparty.Name.ToLower().Contains(searchLower)));

            return await query
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }
        public async Task DeleteEventAsync(int id)
        {
            var documentEvent = await _context.DocumentEvents.FindAsync(id);
            if (documentEvent != null)
            {
                _context.DocumentEvents.Remove(documentEvent);
                await _context.SaveChangesAsync();
            }
        }
    }
}