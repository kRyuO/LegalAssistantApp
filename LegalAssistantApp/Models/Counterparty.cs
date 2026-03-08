using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LegalAssistantApp.Models
{
    public class Counterparty
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Тип контрагента: Юр.лицо / Физ.лицо
        /// </summary>
        [MaxLength(20)]
        public string Type { get; set; } = "Юр.лицо";

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(12)]
        public string INN { get; set; } = string.Empty;

        [MaxLength(9)]
        public string KPP { get; set; } = string.Empty;

        [MaxLength(15)]
        public string OGRN { get; set; } = string.Empty;

        [MaxLength(500)]
        public string LegalAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string ActualAddress { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Основное контактное лицо
        /// </summary>
        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string DirectorName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(50)]
        public string RiskLevel { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Notes { get; set; } = string.Empty;

        // Связи
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

        // История аудита
        public virtual ICollection<AuditHistory> AuditHistories { get; set; } = new List<AuditHistory>();

        // Системные поля
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}