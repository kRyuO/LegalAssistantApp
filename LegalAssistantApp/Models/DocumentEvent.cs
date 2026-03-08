using System;
using System.ComponentModel.DataAnnotations;

namespace LegalAssistantApp.Models
{
    public class DocumentEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [MaxLength(50)]
        public string EventType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }

        public int? DocumentId { get; set; }
        public virtual Document? Document { get; set; }

        public int? CounterpartyId { get; set; }
        public virtual Counterparty? Counterparty { get; set; }

        public bool HasReminder { get; set; }
        public DateTime? ReminderDate { get; set; }

        /// <summary>
        /// Устаревшее поле, используется для миграции старых данных.
        /// Для новых напоминаний применяется ReminderOffsetMinutes.
        /// </summary>
        public int? ReminderDaysBefore { get; set; }

        /// <summary>
        /// Смещение напоминания в минутах относительно даты события.
        /// Примеры:
        ///  - 5  минут
        ///  - 60 минут (1 час)
        ///  - 1440 минут (1 день)
        ///  - 10080 минут (неделя)
        /// </summary>
        public int? ReminderOffsetMinutes { get; set; }

        [MaxLength(200)]
        public string NotificationEmail { get; set; } = string.Empty;

        /// <summary>
        /// Флаг, что Email-уведомление по этому напоминанию уже отправлено.
        /// </summary>
        public bool ReminderEmailSent { get; set; }

        public DateTime? ReminderLastSentDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; } = string.Empty;

        // Для совместимости
        public DateTime? DueDate
        {
            get => EventDate;
            set => EventDate = value ?? DateTime.Now;
        }

        public int? Priority { get; set; }
    }
}