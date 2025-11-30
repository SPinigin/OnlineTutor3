namespace OnlineTutor3.Domain.Entities
{
    /// <summary>
    /// Событие календаря (занятие)
    /// </summary>
    public class CalendarEvent
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        // Связь с учителем
        public string TeacherId { get; set; } = string.Empty;

        // Занятие может быть для класса
        public int? ClassId { get; set; }

        // Или для отдельного ученика
        public int? StudentId { get; set; }

        public string? Location { get; set; } // Место (онлайн, кабинет и т.д.)

        public string? Color { get; set; } // Цвет для календаря

        public bool IsRecurring { get; set; } // Повторяющееся событие

        public string? RecurrencePattern { get; set; } // Паттерн повторения (daily, weekly, biweekly, monthly)

        public bool IsCompleted { get; set; } // Занятие завершено

        public string? Notes { get; set; } // Заметки после занятия

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

