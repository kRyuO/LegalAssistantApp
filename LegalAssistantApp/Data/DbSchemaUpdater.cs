using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LegalAssistantApp.Data;

public static class DbSchemaUpdater
{
    public static async Task EnsureUpToDateAsync(AppDbContext context)
    {
        // БД уже может существовать (EnsureCreated не делает миграций).
        // Поэтому добавляем недостающие колонки/таблицы вручную.
        await context.Database.EnsureCreatedAsync();

        await EnsureDocumentEventsColumnsAsync(context);
        await EnsureCounterpartiesColumnsAsync(context);
        await EnsureEmailLogsTableAsync(context);
    }

    private static async Task EnsureDocumentEventsColumnsAsync(AppDbContext context)
    {
        var table = "DocumentEvents";
        var cols = await GetColumnsAsync(context, table);

        // Новые поля напоминаний / email
        await AddColumnIfMissingAsync(context, cols, table, "ReminderOffsetMinutes", "INTEGER NULL");
        await AddColumnIfMissingAsync(context, cols, table, "ReminderEmailSent", "INTEGER NOT NULL DEFAULT 0");
        await AddColumnIfMissingAsync(context, cols, table, "ReminderLastSentDate", "TEXT NULL");
    }

    private static async Task EnsureCounterpartiesColumnsAsync(AppDbContext context)
    {
        var table = "Counterparties";
        var cols = await GetColumnsAsync(context, table);

        await AddColumnIfMissingAsync(context, cols, table, "Type", "TEXT NOT NULL DEFAULT 'Юр.лицо'");
        await AddColumnIfMissingAsync(context, cols, table, "ContactPerson", "TEXT NOT NULL DEFAULT ''");
    }

    private static async Task EnsureEmailLogsTableAsync(AppDbContext context)
    {
        // Если таблицы нет — создадим простым SQL.
        // (SQLite не поддерживает IF NOT EXISTS для ADD COLUMN, но поддерживает для CREATE TABLE)
        await context.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS EmailLogs (
    Id INTEGER NOT NULL CONSTRAINT PK_EmailLogs PRIMARY KEY AUTOINCREMENT,
    ""To"" TEXT NOT NULL,
    Subject TEXT NOT NULL,
    Body TEXT NOT NULL,
    AttachmentPath TEXT NULL,
    DocumentId INTEGER NULL,
    DocumentEventId INTEGER NULL,
    SentAt TEXT NOT NULL,
    Status TEXT NOT NULL,
    ErrorMessage TEXT NULL
);");
    }

    private static async Task<HashSet<string>> GetColumnsAsync(AppDbContext context, string table)
    {
        // PRAGMA table_info возвращает: cid, name, type, notnull, dflt_value, pk
        var sql = "PRAGMA table_info(" + table + ");";
        var rows = await context.Database
            .SqlQueryRaw<TableInfoRow>(sql)
            .ToListAsync();

        return rows
            .Select(r => r.name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static async Task AddColumnIfMissingAsync(
        AppDbContext context,
        HashSet<string> existingColumns,
        string table,
        string column,
        string sqlType)
    {
        if (existingColumns.Contains(column))
            return;

        var sql = "ALTER TABLE " + table + " ADD COLUMN " + column + " " + sqlType + ";";
        await context.Database.ExecuteSqlRawAsync(sql);
        existingColumns.Add(column);
    }

    // Маппинг для PRAGMA table_info
    private sealed class TableInfoRow
    {
        public int cid { get; set; }
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public int notnull { get; set; }
        public string? dflt_value { get; set; }
        public int pk { get; set; }
    }
}

