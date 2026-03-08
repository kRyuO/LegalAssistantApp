using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegalAssistantApp.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = "";

        [MaxLength(100)]
        public string DocumentNumber { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = "";

        [MaxLength(50)]
        public string Status { get; set; } = "";

        public DateTime? DocumentDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }
        public string Type { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Currency { get; set; } = "RUB";

        [MaxLength(500)]
        public string Tags { get; set; } = "";

        public bool IsConfidential { get; set; }

        [MaxLength(2000)]
        public string Content { get; set; } = "";

        // Связи
        public int? CounterpartyId { get; set; }
        public virtual Counterparty? Counterparty { get; set; }

        // Навигационное свойство для событий
        public virtual ICollection<DocumentEvent> Events { get; set; } = new List<DocumentEvent>();

        public int CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; } // Добавлено

        // Поля для файлов (опционально)
        [MaxLength(500)]
        public string FilePath { get; set; } = "";

        [MaxLength(100)]
        public string FileName { get; set; } = "";

        public long? FileSize { get; set; }

        public DateTime? FileCreatedDate { get; set; }

        public DateTime? FileModifiedDate { get; set; }

        [MaxLength(50)]
        public string FileExtension { get; set; } = "";
    }
}