using Microsoft.EntityFrameworkCore;
using LegalAssistantApp.Models;

namespace LegalAssistantApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Counterparty> Counterparties { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentEvent> DocumentEvents { get; set; }
        public DbSet<AuditHistory> AuditHistories { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=legal_assistant.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация пользователей и ролей
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Конфигурация документов
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Counterparty)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CounterpartyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Конфигурация событий документов
            modelBuilder.Entity<DocumentEvent>()
                .HasOne(e => e.Document)
                .WithMany(d => d.Events)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentEvent>()
                .HasOne(e => e.Counterparty)
                .WithMany()
                .HasForeignKey(e => e.CounterpartyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Индексы для оптимизации
            modelBuilder.Entity<Document>()
                .HasIndex(d => d.Title);

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.DocumentNumber);

            modelBuilder.Entity<Document>()
                .HasIndex(d => d.DocumentDate);

            modelBuilder.Entity<Counterparty>()
                .HasIndex(c => c.INN);

            modelBuilder.Entity<Counterparty>()
                .HasIndex(c => c.Name);

            modelBuilder.Entity<DocumentEvent>()
                .HasIndex(e => e.EventDate);

            modelBuilder.Entity<DocumentEvent>()
                .HasIndex(e => e.IsCompleted);
        }
    }
}