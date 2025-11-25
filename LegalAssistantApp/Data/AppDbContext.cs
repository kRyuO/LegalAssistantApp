using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegalAssistantApp.Models;

namespace LegalAssistantApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Counterparty> Counterparties { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
    public DbSet<AuditHistory> AuditHistories { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<DocumentEvent> DocumentEvents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=legal_assistant.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<Counterparty>()
            .HasIndex(c => c.INN)
            .IsUnique();

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.DocumentNumber);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.Timestamp);

        modelBuilder.Entity<DocumentEvent>()
            .HasIndex(e => e.DueDate);

        modelBuilder.Entity<Counterparty>()
            .HasMany(c => c.Documents)
            .WithOne(d => d.Counterparty)
            .OnDelete(DeleteBehavior.Restrict);
    }
}